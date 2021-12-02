module site.App

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Options

[<CLIMutable>]
type Settings = { MyValue: int }

let demo = 
    fun (next : HttpFunc) (ctx : HttpContext) ->

        let settings = ctx.GetService<IOptions<Settings>>()

        let greeting = sprintf "hello world %d" settings.Value.MyValue
        text greeting next ctx

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> demo
            ] ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =

    services.AddGiraffe() |> ignore

    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    services.Configure<Settings>(settings.GetSection("MySite")) |> ignore
  

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0