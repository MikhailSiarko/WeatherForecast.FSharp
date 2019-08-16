namespace WeatherForecast.FSharp.API.Modules

open WeatherForecast.FSharp.Domain

module WeatherForecast =
    let private requestForecastAsync apiKey location = async {
        return! ForecastSource.getAsync apiKey location
                |> Async.RunSynchronously
                |> ForecastStorage.saveAsync  
    }
    
    let private requestUpdateAsync apiKey forecast = async {
        let! update = ForecastSource.getAsync apiKey forecast.City
        return! ForecastStorage.saveAsync { forecast with Items = update.Items }
    }
    
    let getAsync apiKey expirationTime location = async {
        let! forecastOption = ForecastStorage.tryGetAsync location
        return match forecastOption with
               | Some f -> match Forecast.validate (f, expirationTime) with
                           | Ok f -> f
                           | Expired f -> requestUpdateAsync apiKey f
                                          |> Async.RunSynchronously
               | None -> requestForecastAsync apiKey location
                         |> Async.RunSynchronously
    }
