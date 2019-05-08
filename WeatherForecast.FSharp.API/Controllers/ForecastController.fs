namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open WeatherForecast.FSharp.API.Types

[<Route("api/[controller]")>]
[<ApiController>]
[<Authorize>]
type ForecastController (getForecastAsync: GetForecast) =
    inherit ControllerBase()
    
    [<HttpGet("{city}")>]
    member __.Get([<FromRoute>] city: string) = async {
        let! forecast = getForecastAsync city
        return JsonResult(forecast)
    }