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


let createTestHost () =
  WebHostBuilder()
    .UseTestServer()
    .Configure(configureApp)
    .ConfigureServices(configureServices)
    

[<Fact>]
let ``My test`` () =
    task {
        use server = new TestServer(createTestHost())

        use msg = new HttpRequestMessage(HttpMethod.Get, "/")

        use client = server.CreateClient()
        use! response = client.SendAsync msg
        let! content = response.Content.ReadAsStringAsync()

        let expected = "hello test"

        Assert.Equal(expected, content)
    }
    
