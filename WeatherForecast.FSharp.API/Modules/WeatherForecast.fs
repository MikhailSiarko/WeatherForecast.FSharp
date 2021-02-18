namespace WeatherForecast.FSharp.API.Modules

open System
open WeatherForecast.FSharp.Domain
open WeatherForecast.FSharp.Storage

module WeatherForecast =
    let private requestForecastAsync apiKey location =
        async {
            return!
                ForecastProvider.getAsync apiKey location
                |> Async.RunSynchronously
                |> ForecastStorage.saveAsync
        }

    let private requestUpdateAsync apiKey forecastName =
        async { return! ForecastProvider.getAsync apiKey forecastName }

    let getAsync apiKey (expiredAfter: float<min>) location =
        async {
            match! ForecastStorage.tryGetAsync location with
            | Some f ->
                match Forecast.validate f.Updated (DateTime.UtcNow.AddMinutes(-1.0 * float expiredAfter)) with
                | Valid -> return f
                | Expired ->
                    let! forecastSource = ForecastProvider.getAsync apiKey f.Name
                    return! ForecastStorage.saveAsync { forecastSource with Id = f.Id }
            | None -> return! requestForecastAsync apiKey location
        }
