namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open WeatherForecast.FSharp.API.Types

[<Route("api/[controller]")>]
[<ApiController>]
[<Authorize>]
type ForecastController (fetchAsync: FetchForecast) =
    inherit ControllerBase()
    
    [<HttpGet("{city}")>]
    member __.Get([<FromRoute>] city: string) = async {
        let! forecast = fetchAsync city
        return JsonResult(forecast)
    }