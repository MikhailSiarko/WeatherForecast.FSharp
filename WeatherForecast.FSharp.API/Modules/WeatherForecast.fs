namespace WeatherForecast.FSharp.API.Modules

open System
open Microsoft.Extensions.Configuration
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

type WeatherForecast (configuration: IConfiguration, weatherService: WeatherAPIService) =
    let expirationTime = float (configuration.GetSection("ExpirationTime").Value)
    member __.LoadAsync countryCode city = async {
        let lastEligibleTime = DateTimeOffset.Now.AddMinutes(-1.0 * expirationTime).DateTime
        let! forecast = Database.tryGetForecastAsync countryCode city
        match forecast with
        | Some f when f.Created >= lastEligibleTime -> return Mapping.toForecast f
        | Some f -> Database.clearUpdates ()
                    do! Database.deleteItemsAsync f.Id
                    do! Database.createItemsAsync weatherService.Load countryCode city f.Id
                    do! Database.updateAsync f
                    return Mapping.toForecast f
        | None -> Database.clearUpdates ()
                  let! forecast = Database.createForecastAsync countryCode city
                  do! Database.createItemsAsync weatherService.Load countryCode city forecast.Id
                  return Mapping.toForecast forecast
    }