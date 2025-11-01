module LinkInBio.Backend.Handlers.AnalyticsHandlers

open System
open Giraffe
open Microsoft.AspNetCore.Http
open LinkInBio.Backend.Models.DTOs
open LinkInBio.Backend.Database.DbContext
open LinkInBio.Backend.Services.AnalyticsService
open LinkInBio.Backend.Database.DbContext

// Helper to get user ID from JWT token
let private getUserIdFromToken (ctx: HttpContext) : Guid option =
    match ctx.Request.Headers.TryGetValue("Authorization") with
    | false, _ -> None
    | true, authHeader ->
        let token = authHeader.ToString().Replace("Bearer ", "")
        match AuthService.validateToken token with
        | Ok userId -> Some userId
        | Error _ -> None

// Helper to verify profile ownership
let private verifyProfileOwnership (userId: Guid) (profileId: Guid) =
    async {
        let! profiles = Profiles.findByUserId userId
        return profiles |> List.exists (fun p -> p.Id = profileId)
    }

// POST /api/analytics/click (public endpoint - for tracking clicks)
let trackClick : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! request = ctx.BindJsonAsync<ClickEventRequest>()

            // Find the link to get its profile ID
            let! allProfilesTask =
                async {
                    // This is simplified - in production, you'd want a more efficient query
                    let connectionString = DbContext.getConnectionString()
                    return! Npgsql.FSharp.Sql.connect connectionString
                            |> Npgsql.FSharp.Sql.query "SELECT profile_id FROM links WHERE id = @linkId"
                            |> Npgsql.FSharp.Sql.parameters [ "@linkId", Npgsql.FSharp.Sql.uuid request.LinkId ]
                            |> Npgsql.FSharp.Sql.executeAsync (fun read -> read.uuid "profile_id")
                }

            let! profiles = allProfilesTask

            match profiles with
            | [] -> return! RequestErrors.NOT_FOUND "Link not found" next ctx
            | profileId :: _ ->
                let! result = Services.AnalyticsService.trackClick request profileId

                match result with
                | Ok () -> return! Successful.OK {| message = "Click tracked successfully" |} next ctx
                | Error msg -> return! ServerErrors.INTERNAL_ERROR msg next ctx
        }

// GET /api/analytics/:profileId (authenticated)
let getAnalytics (profileId: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                match Guid.TryParse(profileId) with
                | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid profile ID" next ctx
                | true, profileGuid ->
                    let! hasAccess = verifyProfileOwnership userId profileGuid

                    if not hasAccess then
                        return! RequestErrors.FORBIDDEN "Access denied" next ctx
                    else
                        // Parse query parameters for date range
                        let startDateStr = ctx.TryGetQueryStringValue "startDate"
                        let endDateStr = ctx.TryGetQueryStringValue "endDate"

                        let dateRange =
                            match startDateStr, endDateStr with
                            | Some start, Some end_ ->
                                match DateTime.TryParse(start), DateTime.TryParse(end_) with
                                | (true, startDate), (true, endDate) ->
                                    Some { StartDate = Some startDate; EndDate = Some endDate }
                                | _ -> None
                            | _ -> None

                        let! result = Services.AnalyticsService.getAnalytics profileGuid dateRange

                        match result with
                        | Ok analytics -> return! Successful.OK analytics next ctx
                        | Error msg -> return! ServerErrors.INTERNAL_ERROR msg next ctx
        }

// GET /api/analytics/:profileId/export (authenticated - export to CSV)
let exportAnalytics (profileId: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                match Guid.TryParse(profileId) with
                | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid profile ID" next ctx
                | true, profileGuid ->
                    let! hasAccess = verifyProfileOwnership userId profileGuid

                    if not hasAccess then
                        return! RequestErrors.FORBIDDEN "Access denied" next ctx
                    else
                        let! result = Services.AnalyticsService.exportToCSV profileGuid None

                        match result with
                        | Ok csv ->
                            ctx.SetContentType "text/csv"
                            ctx.SetHttpHeader("Content-Disposition", $"attachment; filename=\"analytics-{profileId}.csv\"")
                            return! Successful.OK csv next ctx
                        | Error msg -> return! ServerErrors.INTERNAL_ERROR msg next ctx
        }

// GET /api/analytics/:profileId/top-links (authenticated)
let getTopLinks (profileId: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                match Guid.TryParse(profileId) with
                | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid profile ID" next ctx
                | true, profileGuid ->
                    let! hasAccess = verifyProfileOwnership userId profileGuid

                    if not hasAccess then
                        return! RequestErrors.FORBIDDEN "Access denied" next ctx
                    else
                        let limit =
                            ctx.TryGetQueryStringValue "limit"
                            |> Option.bind (fun s -> match Int32.TryParse(s) with true, v -> Some v | _ -> None)
                            |> Option.defaultValue 10

                        let! result = Services.AnalyticsService.getTopLinks profileGuid limit

                        match result with
                        | Ok topLinks ->
                            let response =
                                topLinks
                                |> List.map (fun (linkId, clicks) -> {| LinkId = linkId; Clicks = clicks |})
                            return! Successful.OK response next ctx
                        | Error msg -> return! ServerErrors.INTERNAL_ERROR msg next ctx
        }
