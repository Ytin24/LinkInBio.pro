module LinkInBio.Backend.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text

// Import handlers
open LinkInBio.Backend.Handlers

// CORS configuration
let configureCors (services: IServiceCollection) =
    services.AddCors(fun options ->
        options.AddPolicy("AllowAll", fun builder ->
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            |> ignore
        )
    ) |> ignore

// JWT Authentication configuration
let configureAuth (services: IServiceCollection) =
    let jwtSecret =
        Environment.GetEnvironmentVariable("JWT_SECRET")
        |> Option.ofObj
        |> Option.defaultValue "your-super-secret-jwt-key-change-this-in-production"

    let key = Encoding.ASCII.GetBytes(jwtSecret)

    services
        .AddAuthentication(fun options ->
            options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
        )
        .AddJwtBearer(fun options ->
            options.RequireHttpsMetadata <- false
            options.SaveToken <- true
            options.TokenValidationParameters <- TokenValidationParameters(
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "LinkInBio.pro",
                ValidateAudience = true,
                ValidAudience = "LinkInBio.pro",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            )
        )
    |> ignore

// API Routes
let webApp =
    choose [
        // Health check
        GET >=> route "/health" >=> text "OK"

        // Public routes (no authentication required)
        subRoute "/api" (
            choose [
                // Auth routes
                POST >=> route "/auth/register" >=> AuthHandlers.register
                POST >=> route "/auth/login" >=> AuthHandlers.login

                // Public profile view
                GET >=> routef "/profiles/%s" ProfileHandlers.getBySlug

                // Analytics - track click (public)
                POST >=> route "/analytics/click" >=> AnalyticsHandlers.trackClick
            ]
        )

        // Authenticated routes
        subRoute "/api" (
            choose [
                // Auth - get current user
                GET >=> route "/auth/me" >=> AuthHandlers.getCurrentUser

                // Profile management (authenticated)
                GET >=> route "/profiles/my" >=> ProfileHandlers.getMyProfiles
                POST >=> route "/profiles" >=> ProfileHandlers.create
                PUT >=> routef "/profiles/%s" ProfileHandlers.update

                // Link management (authenticated)
                POST >=> routef "/profiles/%s/links" LinkHandlers.create
                GET >=> routef "/profiles/%s/links" LinkHandlers.getAll
                PUT >=> routef "/links/%s" LinkHandlers.update
                DELETE >=> routef "/links/%s" LinkHandlers.delete

                // Analytics (authenticated)
                GET >=> routef "/analytics/%s" AnalyticsHandlers.getAnalytics
                GET >=> routef "/analytics/%s/export" AnalyticsHandlers.exportAnalytics
                GET >=> routef "/analytics/%s/top-links" AnalyticsHandlers.getTopLinks
            ]
        )

        // 404
        RequestErrors.NOT_FOUND "Route not found"
    ]

// Error handler
let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception occurred")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// Configure services
let configureServices (services: IServiceCollection) =
    configureCors services
    configureAuth services
    services.AddGiraffe() |> ignore

// Configure application
let configureApp (app: IApplicationBuilder) =
    app.UseCors("AllowAll")
       .UseAuthentication()
       .UseGiraffe(webApp)

// Entry point
[<EntryPoint>]
let main args =
    let port =
        Environment.GetEnvironmentVariable("PORT")
        |> Option.ofObj
        |> Option.bind (fun s -> match Int32.TryParse(s) with true, v -> Some v | _ -> None)
        |> Option.defaultValue 5000

    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls($"http://0.0.0.0:{port}")
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore
        )
        .Build()
        .Run()

    0
