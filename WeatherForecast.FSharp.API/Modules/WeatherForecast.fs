namespace WeatherForecast.FSharp.API.Modules

open System
open Microsoft.Extensions.Configuration
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

type WeatherForecast (configuration: IConfiguration, weatherService: WeatherAPIService) =
    let expirationTime = float (configuration.GetSection("ExpirationTime").Value)
    let updateForecastAsync = Database.updateAsync weatherService.Load
    let createForecastAsync = Database.createForecastAsync weatherService.Load
    let processRequest lastEligibleTime countryCode city (option: ForecastEntity option) = async {
        match option with
        | Some f when f.Created >= lastEligibleTime -> return f
        | Some f -> do! updateForecastAsync f
                    return f
        | None -> let! forecast = createForecastAsync countryCode city
                  return forecast
    }

    member __.LoadAsync countryCode city = async {
        let lastEligibleTime = DateTimeOffset.Now.AddMinutes(-1.0 * expirationTime).DateTime
        let! forecastOption = Database.tryGetForecastAsync countryCode city
        return forecastOption
                |> processRequest lastEligibleTime countryCode city
                |> Async.RunSynchronously
                |> Mapping.toForecast
    }