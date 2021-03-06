module site.App

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Giraffe

[<CLIMutable>]
type Settings = { MyValue: int }

type IMyService =
    abstract member GetNumber : unit -> int

type RealMyService() =
    interface IMyService with
        member _.GetNumber() = 42

let demo = 
    fun (next : HttpFunc) (ctx : HttpContext) ->

        let settings = ctx.GetService<IOptions<Settings>>()
        let myService = ctx.GetService<IMyService>()

        let configNo = settings.Value.MyValue
        let serviceNo = myService.GetNumber()

        let greeting = sprintf "hello world %d %d" configNo serviceNo
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

    services.AddSingleton<IMyService>(new RealMyService()) |> ignore

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