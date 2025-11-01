module LinkInBio.Backend.Handlers.AuthHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open LinkInBio.Backend.Models.DTOs
open LinkInBio.Backend.Services.AuthService

// POST /api/auth/register
let register : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! request = ctx.BindJsonAsync<RegisterRequest>()

            // Validate input
            if System.String.IsNullOrWhiteSpace(request.Email) then
                return! RequestErrors.BAD_REQUEST "Email is required" next ctx
            elif System.String.IsNullOrWhiteSpace(request.Password) then
                return! RequestErrors.BAD_REQUEST "Password is required" next ctx
            elif request.Password.Length < 8 then
                return! RequestErrors.BAD_REQUEST "Password must be at least 8 characters" next ctx
            elif System.String.IsNullOrWhiteSpace(request.Username) then
                return! RequestErrors.BAD_REQUEST "Username is required" next ctx
            else
                let! result = AuthService.register request

                match result with
                | Ok response -> return! Successful.CREATED response next ctx
                | Error msg -> return! RequestErrors.BAD_REQUEST msg next ctx
        }

// POST /api/auth/login
let login : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! request = ctx.BindJsonAsync<LoginRequest>()

            let! result = AuthService.login request

            match result with
            | Ok response -> return! Successful.OK response next ctx
            | Error msg -> return! RequestErrors.UNAUTHORIZED "Bearer" "" msg next ctx
        }

// GET /api/auth/me
let getCurrentUser : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match ctx.Request.Headers.TryGetValue("Authorization") with
            | false, _ -> return! RequestErrors.UNAUTHORIZED "Bearer" "" "Authorization header missing" next ctx
            | true, authHeader ->
                let token = authHeader.ToString().Replace("Bearer ", "")

                let! result = AuthService.getUserFromToken token

                match result with
                | Ok user ->
                    let response = {|
                        Id = user.Id
                        Email = user.Email
                        Username = user.Username
                        SubscriptionTier = string user.SubscriptionTier
                    |}
                    return! Successful.OK response next ctx
                | Error msg -> return! RequestErrors.UNAUTHORIZED "Bearer" "" msg next ctx
        }
