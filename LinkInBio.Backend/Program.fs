module LinkInBio.Backend.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe

// Simple health check route
let webApp =
    choose [
        GET >=> route "/health" >=> text "OK"
        GET >=> route "/" >=> text "LinkInBio.pro API v0.1.0"
    ]

// Configure services
let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

// Configure application  
let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe(webApp)

// Entry point
[<EntryPoint>]
let main args =
    let port =
        Environment.GetEnvironmentVariable("PORT")
        |> Option.ofObj
        |> Option.bind (fun s -> match Int32.TryParse(s) with true, v -> Some v | _ -> None)
        |> Option.defaultValue 5000

    printfn "Starting LinkInBio.pro API on port %d..." port

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
