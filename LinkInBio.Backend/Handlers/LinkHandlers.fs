module LinkInBio.Backend.Handlers.LinkHandlers

open System
open Giraffe
open Microsoft.AspNetCore.Http
open LinkInBio.Backend.Models.Domain
open LinkInBio.Backend.Models.DTOs
open LinkInBio.Backend.Database.DbContext

// Helper to get user ID from JWT token
let private getUserIdFromToken (ctx: HttpContext) : Guid option =
    match ctx.Request.Headers.TryGetValue("Authorization") with
    | false, _ -> None
    | true, authHeader ->
        let token = authHeader.ToString().Replace("Bearer ", "")
        match Services.AuthService.validateToken token with
        | Ok userId -> Some userId
        | Error _ -> None

// Helper to verify profile ownership
let private verifyProfileOwnership (userId: Guid) (profileId: Guid) =
    async {
        let! profiles = Profiles.findByUserId userId
        return profiles |> List.exists (fun p -> p.Id = profileId)
    }

// POST /api/profiles/:profileId/links (authenticated)
let create (profileId: string) : HttpHandler =
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
                        let! request = ctx.BindJsonAsync<CreateLinkRequest>()

                        // Validate URL format
                        if not (Uri.IsWellFormedUriString(request.Url, UriKind.Absolute)) then
                            return! RequestErrors.BAD_REQUEST "Invalid URL format" next ctx
                        else
                            let! existingLinks = Links.findByProfileId profileGuid
                            let nextPosition =
                                request.Position
                                |> Option.defaultValue (existingLinks.Length + 1)

                            let newLink = {
                                Id = Guid.NewGuid()
                                ProfileId = profileGuid
                                Title = request.Title
                                Url = request.Url
                                IconUrl = request.IconUrl
                                Position = nextPosition
                                IsActive = true
                                CreatedAt = DateTime.UtcNow
                            }

                            try
                                let! _ = Links.create newLink

                                let response : LinkResponse = {
                                    Id = newLink.Id
                                    Title = newLink.Title
                                    Url = newLink.Url
                                    IconUrl = newLink.IconUrl
                                    Position = newLink.Position
                                    IsActive = newLink.IsActive
                                }

                                return! Successful.CREATED response next ctx
                            with
                            | ex -> return! ServerErrors.INTERNAL_ERROR ex.Message next ctx
        }

// GET /api/profiles/:profileId/links (public)
let getAll (profileId: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match Guid.TryParse(profileId) with
            | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid profile ID" next ctx
            | true, profileGuid ->
                let! links = Links.findByProfileId profileGuid

                let responses =
                    links
                    |> List.filter (fun l -> l.IsActive)
                    |> List.map (fun link -> {
                        Id = link.Id
                        Title = link.Title
                        Url = link.Url
                        IconUrl = link.IconUrl
                        Position = link.Position
                        IsActive = link.IsActive
                    })

                return! Successful.OK responses next ctx
        }

// PUT /api/links/:id (authenticated)
let update (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                match Guid.TryParse(id) with
                | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid link ID" next ctx
                | true, linkId ->
                    // First get the link to find its profile
                    let! allProfiles = Profiles.findByUserId userId

                    let! linkOpt =
                        async {
                            for profile in allProfiles do
                                let! links = Links.findByProfileId profile.Id
                                match links |> List.tryFind (fun l -> l.Id = linkId) with
                                | Some link -> return Some link
                                | None -> ()
                            return None
                        }

                    match linkOpt with
                    | None -> return! RequestErrors.NOT_FOUND "Link not found or access denied" next ctx
                    | Some link ->
                        let! request = ctx.BindJsonAsync<UpdateLinkRequest>()

                        let updatedLink = {
                            link with
                                Title = request.Title |> Option.defaultValue link.Title
                                Url = request.Url |> Option.defaultValue link.Url
                                IconUrl = request.IconUrl |> Option.orElse link.IconUrl
                                Position = request.Position |> Option.defaultValue link.Position
                                IsActive = request.IsActive |> Option.defaultValue link.IsActive
                        }

                        try
                            let! _ = Links.update updatedLink

                            let response : LinkResponse = {
                                Id = updatedLink.Id
                                Title = updatedLink.Title
                                Url = updatedLink.Url
                                IconUrl = updatedLink.IconUrl
                                Position = updatedLink.Position
                                IsActive = updatedLink.IsActive
                            }

                            return! Successful.OK response next ctx
                        with
                        | ex -> return! ServerErrors.INTERNAL_ERROR ex.Message next ctx
        }

// DELETE /api/links/:id (authenticated)
let delete (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                match Guid.TryParse(id) with
                | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid link ID" next ctx
                | true, linkId ->
                    let! allProfiles = Profiles.findByUserId userId

                    let! hasAccess =
                        async {
                            for profile in allProfiles do
                                let! links = Links.findByProfileId profile.Id
                                if links |> List.exists (fun l -> l.Id = linkId) then
                                    return true
                            return false
                        }

                    if not hasAccess then
                        return! RequestErrors.NOT_FOUND "Link not found or access denied" next ctx
                    else
                        try
                            let! _ = Links.delete linkId
                            return! Successful.NO_CONTENT next ctx
                        with
                        | ex -> return! ServerErrors.INTERNAL_ERROR ex.Message next ctx
        }
