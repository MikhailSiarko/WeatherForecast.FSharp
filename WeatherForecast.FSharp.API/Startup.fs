namespace WeatherForecast.FSharp.API

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.DependencyInjection
open WeatherForecast.FSharp.API.Infrastructure

type Startup () =
    member __.ConfigureServices(services: IServiceCollection) =
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore
        services.AddApplicationAuthentication() |> ignore

    member __.Configure(app: IApplicationBuilder) =
        app.UseMiddleware<ExceptionHandlingMiddleware>()
            .ConfigureApplicationCors()
            .UseHttpsRedirection()
            .UseAuthentication()
            .UseMvc() |> ignore