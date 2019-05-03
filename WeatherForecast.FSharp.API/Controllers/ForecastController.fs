namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open WeatherForecast.FSharp.API.Modules

[<Route("api/[controller]")>]
[<ApiController>]
[<Authorize>]
type ForecastController (weatherForecast: WeatherForecast) =
    inherit ControllerBase()
    
    [<HttpGet("{countryCode}/{city}")>]
    member __.Get([<FromRoute>] countryCode: string, [<FromRoute>] city: string) = async {
        let! forecast = weatherForecast.LoadAsync countryCode city
        return JsonResult(forecast)
    }