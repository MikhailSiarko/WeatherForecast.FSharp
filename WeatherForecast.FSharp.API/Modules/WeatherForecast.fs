namespace WeatherForecast.FSharp.API.Modules

open WeatherForecast.FSharp.Domain

module WeatherForecast =
    let private requestForecastAsync apiKey location = async {
        return! ForecastSource.getAsync apiKey location
                |> Async.RunSynchronously
                |> ForecastStorage.saveAsync  
    }
    
    let getAsync apiKey expirationTime location = async {
        let! forecastOption = ForecastStorage.tryGetAsync location
        return match forecastOption with
               | Some f -> match Forecast.validate (f, expirationTime) with
                           | Ok f -> f
                           | Expired f -> requestForecastAsync apiKey f.City
                                          |> Async.RunSynchronously
               | None -> requestForecastAsync apiKey location
                         |> Async.RunSynchronously
    }
