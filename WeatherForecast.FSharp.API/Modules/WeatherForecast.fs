namespace WeatherForecast.FSharp.API.Modules

open System
open WeatherForecast.FSharp.Domain
open WeatherForecast.FSharp.Storage

module WeatherForecast =
    let private requestForecastAsync apiKey location = async {
        return! ForecastProvider.getAsync apiKey location
                |> Async.RunSynchronously
                |> ForecastStorage.saveAsync
    }
    
    let private requestUpdateAsync apiKey (expired: ExpiredForecast) = async {
        let (ExpiredForecast forecast) = expired
        let! valid = ForecastProvider.getAsync apiKey forecast.City
        return! valid
                |> Forecast.update expired
                |> ForecastStorage.saveAsync
    }
    
    let getAsync apiKey expiredAfter location = async {
        let! forecastOption = ForecastStorage.tryGetAsync location
        return match forecastOption with
               | Some f -> match Forecast.validate (f, DateTime.UtcNow, expiredAfter) with
                           | Valid(ValidForecast v) -> v
                           | Expired e -> requestUpdateAsync apiKey e
                                          |> Async.RunSynchronously
               | None -> requestForecastAsync apiKey location
                         |> Async.RunSynchronously
    }
