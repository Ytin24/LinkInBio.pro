module LinkInBio.Backend.Handlers.ProfileHandlers

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

// Helper to map Profile to ProfileResponse
let private toProfileResponse (profile: Profile) (links: Link list) : ProfileResponse =
    {
        Id = profile.Id
        Slug = profile.Slug
        DisplayName = profile.DisplayName
        Bio = profile.Bio
        AvatarUrl = profile.AvatarUrl
        Theme = profile.Theme
        Links = links |> List.map (fun link -> {
            Id = link.Id
            Title = link.Title
            Url = link.Url
            IconUrl = link.IconUrl
            Position = link.Position
            IsActive = link.IsActive
        })
        IsPublished = profile.IsPublished
    }

// GET /api/profiles/:slug (public endpoint)
let getBySlug (slug: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! profiles = Profiles.findBySlug slug

            match profiles with
            | [] -> return! RequestErrors.NOT_FOUND $"Profile not found: {slug}" next ctx
            | profile :: _ ->
                if not profile.IsPublished then
                    return! RequestErrors.NOT_FOUND "Profile not published" next ctx
                else
                    let! links = Links.findByProfileId profile.Id
                    let response = toProfileResponse profile links
                    return! Successful.OK response next ctx
        }

// POST /api/profiles (authenticated)
let create : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                let! request = ctx.BindJsonAsync<CreateProfileRequest>()

                // Validate slug format (alphanumeric + hyphens)
                if not (System.Text.RegularExpressions.Regex.IsMatch(request.Slug, "^[a-zA-Z0-9-]+$")) then
                    return! RequestErrors.BAD_REQUEST "Slug must contain only letters, numbers, and hyphens" next ctx
                else
                    // Check if slug is already taken
                    let! existingProfiles = Profiles.findBySlug request.Slug

                    match existingProfiles with
                    | _ :: _ -> return! RequestErrors.CONFLICT "Slug already taken" next ctx
                    | [] ->
                        let newProfile = {
                            Id = Guid.NewGuid()
                            UserId = userId
                            Slug = request.Slug
                            DisplayName = request.DisplayName
                            Bio = request.Bio
                            AvatarUrl = None
                            Theme = {
                                BackgroundColor = "#ffffff"
                                TextColor = "#000000"
                                ButtonStyle = Rounded
                                FontFamily = "Inter"
                            }
                            IsPublished = false
                            CreatedAt = DateTime.UtcNow
                            UpdatedAt = DateTime.UtcNow
                        }

                        try
                            let! _ = Profiles.create newProfile
                            let response = toProfileResponse newProfile []
                            return! Successful.CREATED response next ctx
                        with
                        | ex -> return! ServerErrors.INTERNAL_ERROR ex.Message next ctx
        }

// GET /api/profiles/my (authenticated - get all user's profiles)
let getMyProfiles : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                let! profiles = Profiles.findByUserId userId

                let! responses =
                    profiles
                    |> List.map (fun profile ->
                        task {
                            let! links = Links.findByProfileId profile.Id
                            return toProfileResponse profile links
                        })
                    |> Task.WhenAll

                return! Successful.OK (List.ofArray responses) next ctx
        }

// PUT /api/profiles/:id (authenticated)
let update (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match getUserIdFromToken ctx with
            | None -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Invalid token" next ctx
            | Some userId ->
                match Guid.TryParse(id) with
                | false, _ -> return! RequestErrors.BAD_REQUEST "Invalid profile ID" next ctx
                | true, profileId ->
                    let! profiles = Profiles.findByUserId userId
                    let profileOpt = profiles |> List.tryFind (fun p -> p.Id = profileId)

                    match profileOpt with
                    | None -> return! RequestErrors.NOT_FOUND "Profile not found or access denied" next ctx
                    | Some profile ->
                        let! request = ctx.BindJsonAsync<UpdateProfileRequest>()

                        let updatedProfile = {
                            profile with
                                DisplayName = request.DisplayName |> Option.defaultValue profile.DisplayName
                                Bio = request.Bio |> Option.orElse profile.Bio
                                AvatarUrl = request.AvatarUrl |> Option.orElse profile.AvatarUrl
                                Theme = request.Theme |> Option.defaultValue profile.Theme
                                IsPublished = request.IsPublished |> Option.defaultValue profile.IsPublished
                                UpdatedAt = DateTime.UtcNow
                        }

                        try
                            let! _ = Profiles.update updatedProfile
                            let! links = Links.findByProfileId updatedProfile.Id
                            let response = toProfileResponse updatedProfile links
                            return! Successful.OK response next ctx
                        with
                        | ex -> return! ServerErrors.INTERNAL_ERROR ex.Message next ctx
        }
