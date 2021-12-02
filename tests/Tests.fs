module Tests

open site.App
open System.Net.Http
open Xunit
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

let configureAppConfig (app: IConfigurationBuilder) =
  app.AddJsonFile("appsettings.tests.json") |> ignore
  ()

let luckyNumber = 8

type FakeMyService() =
    interface IMyService with
        member _.GetNumber() = luckyNumber

let configureTestServices (services: IServiceCollection) = 
  services.AddSingleton<IMyService>(new FakeMyService()) |> ignore
  ()

let createTestHost () =
  WebHostBuilder()
    .UseTestServer()
    .ConfigureAppConfiguration(configureAppConfig)   // Use the test's config
    .Configure(configureApp)    // from the "Site" project
    .ConfigureServices(configureServices)   // from the "Site" project
    .ConfigureServices(configureTestServices) // mock services after real ones
    
[<Fact>]
let ``My test`` () =
    task {
        use server = new TestServer(createTestHost())

        use msg = new HttpRequestMessage(HttpMethod.Get, "/")

        use client = server.CreateClient()
        use! response = client.SendAsync msg
        let! content = response.Content.ReadAsStringAsync()

        let config = server.Services.GetService(typeof<IConfiguration>) :?> IConfiguration

        let expectedNumber = config.["MySite:MyValue"] |> int

        let expected = sprintf "hello world %d %d" expectedNumber luckyNumber

        Assert.Equal(expected, content)
    }
    
