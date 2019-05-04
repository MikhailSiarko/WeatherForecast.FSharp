namespace WeatherForecast.FSharp.API.Modules

open System
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

module WeatherForecast =
    let private processRequestAsync apiCall lastEligibleTime city (option: ForecastEntity option) = async {
        match option with
        | Some f when f.Created >= lastEligibleTime -> return f
        | Some f -> do! Database.updateAsync apiCall f
                    return f
        | None -> let! forecast = Database.createForecastAsync apiCall city
                  return forecast
    }

    let loadAsync expirationTime apiCall city = async {
        let lastEligibleTime = DateTimeOffset.Now.AddMinutes(-1.0 * expirationTime).DateTime
        let! forecastOption = Database.tryGetForecastAsync city
        return forecastOption
                |> processRequestAsync apiCall lastEligibleTime city
                |> Async.RunSynchronously
                |> Mapping.toForecast
    }