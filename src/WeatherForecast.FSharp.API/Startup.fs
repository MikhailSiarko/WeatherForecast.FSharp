namespace WeatherForecast.FSharp.API

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open WeatherForecast.FSharp.API.Infrastructure

type Startup() =
    member __.ConfigureServices(services: IServiceCollection) =
        services.AddControllers() |> ignore
        services.AddApplicationAuthentication() |> ignore

    member __.Configure(app: IApplicationBuilder) =
        app
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .ConfigureApplicationCors()
            .UseHttpsRedirection()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore)
        |> ignore
