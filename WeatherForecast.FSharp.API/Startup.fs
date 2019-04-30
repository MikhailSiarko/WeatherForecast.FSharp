namespace WeatherForecast.FSharp.API

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.DependencyInjection
open WeatherForecast.FSharp.API.Infrastructure
open WeatherForecast.FSharp.API.Modules
open WeatherForecast.FSharp.API.Types
open Microsoft.Extensions.Configuration

type Startup (configuration: IConfiguration) =
    member __.ConfigureServices(services: IServiceCollection) =
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore
        services.AddScoped<WeatherAPIService>() |> ignore
        services.AddScoped<WeatherForecast>() |> ignore
        services.AddApplicationAuthentication() |> ignore

    member __.Configure(app: IApplicationBuilder) =
        app.UseMiddleware<ExeptionHandlingMiddleware>() |> ignore
        app.ConfigureApplicationCors() |> ignore
        app.UseHttpsRedirection() |> ignore
        app.UseAuthentication() |> ignore
        app.UseMvc() |> ignore