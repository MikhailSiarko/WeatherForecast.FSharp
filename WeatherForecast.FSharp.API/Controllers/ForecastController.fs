namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open WeatherForecast.FSharp.API.Modules

[<Route("api/[controller]")>]
[<ApiController>]
[<Authorize>]
type ForecastController (weatherForecast: WeatherForecast) =
    inherit ControllerBase()
    
    [<HttpGet("{city}")>]
    member __.Get([<FromRoute>] city: string) = async {
        let! forecast = weatherForecast.LoadAsync city
        return JsonResult(forecast)
    }