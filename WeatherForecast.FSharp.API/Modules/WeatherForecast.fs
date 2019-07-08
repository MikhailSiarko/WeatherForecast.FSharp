namespace WeatherForecast.FSharp.API.Modules

open AutoMapper
open System
open System.Linq
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

module WeatherForecast =
    let private createMain (item: ForecastAPI.List) entityId =
        Database.table (fun i -> i.Mains)
        |> Database.add (fun m -> m.ForecastItemId <- entityId
                                  m.Temp <- item.Main.Temp
                                  m.MinTemp <- item.Main.TempMin
                                  m.MaxTemp <- item.Main.TempMax
                                  m.Humidity <- int64 item.Main.Humidity
                                  m.Pressure <- item.Main.Pressure)

    let private createWeatherItems (weathers: ForecastAPI.Weather[]) entityId =
        weathers
        |> Array.map (fun w -> Database.table (fun i -> i.Weathers)
                                |> Database.add (fun i -> i.ForecastItemId <- entityId
                                                          i.Main <- w.Main
                                                          i.Description <- w.Description
                                                          i.Icon <- w.Icon))

    let private createWind (wind: ForecastAPI.Wind) entityId =
        Database.table (fun m -> m.Winds)
        |> Database.add (fun w -> w.ForecastItemId <- entityId
                                  w.Speed <- wind.Speed
                                  w.Degree <- wind.Deg)

    let private createWeatherEntities (item: ForecastAPI.List, entity: ForecastItemEntity) =
        let main = createMain item entity.Id
        let weathers = createWeatherItems item.Weather entity.Id
        let wind = createWind item.Wind entity.Id
        (entity, main, weathers, wind)
    
    let fetchItemsAsync (root: ForecastAPI.Root) (forecast: ForecastEntity) = async {
        root.List
        |> Array.map (fun i -> let entity = Database.table (fun d -> d.ForecastItems)
                                            |> Database.add (fun m -> m.ForecastId <- forecast.Id
                                                                      m.Date <- i.DtTxt)
                               (i, entity))
        |> Database.saveUpdatesAsync
        |> Async.RunSynchronously
        |> Array.map createWeatherEntities
        |> Database.saveUpdatesAsync
        |> Async.RunSynchronously
        |> ignore
        return forecast
    }
    
    let private removeItems (query: IQueryable<ForecastItemEntity>) =
        query
        |> Database.executeAsync
        |> Async.RunSynchronously
        |> Seq.iter (fun i -> Async.RunSynchronously (Database.deleteAsync i.``main.Mains by Id``) |> ignore
                              Async.RunSynchronously (Database.deleteAsync i.``main.Weathers by Id``) |> ignore
                              Async.RunSynchronously (Database.deleteAsync i.``main.Winds by Id``) |> ignore)
        
    
    let private processRequestAsync (fetch: string -> Async<ForecastAPI.Root>) lastEligibleTime city (option: ForecastEntity option) = async {
        match option with
        | Some f when f.Created >= lastEligibleTime -> return f
        | Some f -> let! root = fetch city
                    return! f
                            |> Database.update (fun i -> i.Created <- DateTimeOffset.Now.DateTime)
                            |> Database.update (fun i -> removeItems i.``main.ForecastItems by Id``
                                                         Async.RunSynchronously (Database.deleteAsync i.``main.ForecastItems by Id``) |> ignore)
                            |> Database.saveUpdatesAsync
                            |> Async.RunSynchronously
                            |> fetchItemsAsync root
        | None -> let! root = fetch city
                  return! Database.table (fun i -> i.Forecasts)
                          |> Database.add (fun f -> f.Location <- root.City.Name
                                                    f.CountryCode <- root.City.Country
                                                    f.Created <- DateTimeOffset.Now.DateTime)
                          |> Database.saveUpdatesAsync
                          |> Async.RunSynchronously
                          |> fetchItemsAsync root
    }

    let loadAsync expirationTime fetch (city: string) = async {
        let lastEligibleTime = DateTimeOffset.Now.AddMinutes(-1.0 * expirationTime).DateTime
        let! forecastOption = Database.queryTo <@ fun m -> m.Forecasts :> IQueryable<_> @>
                              |> Database.where <@ fun f -> f.Location.ToLower() = city.ToLower() @>
                              |> Database.singleOrDefaultAsync
        return forecastOption
                |> processRequestAsync fetch lastEligibleTime city
                |> Async.RunSynchronously
                |> AutoMap.mapTo<Forecast>
    }