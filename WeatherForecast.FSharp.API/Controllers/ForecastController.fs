namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open WeatherForecast.FSharp.API.Modules
open WeatherForecast.FSharp.Domain

[<Route("api/[controller]")>]
[<ApiController>]
[<Authorize>]
type ForecastController(configuration: IConfiguration) =
    inherit ControllerBase()

    let apiKey =
        configuration
            .GetSection(
                "WeatherForecastServiceApiKey"
            )
            .Value

    let expirationTime =
        configuration.GetValue<float<min>>("ExpirationTime")

    let getForecastAsync =
        WeatherForecast.getAsync apiKey expirationTime

    [<HttpGet("{city}")>]
    member this.Get([<FromRoute>] city: string) =
        async {
            let! forecast = getForecastAsync city
            return this.Ok(forecast)
        }
