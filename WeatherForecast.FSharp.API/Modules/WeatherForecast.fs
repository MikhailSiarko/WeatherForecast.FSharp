namespace WeatherForecast.FSharp.API.Modules

open AutoMapper
open System
open System.Linq
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

module WeatherForecast =
    let private createMain (item: ForecastAPI.List) timeItemId =
        Database.table (fun i -> i.Mains)
        |> Database.add (fun m -> m.ForecastTimeItemId <- timeItemId
                                  m.Temp <- item.Main.Temp
                                  m.MinTemp <- item.Main.TempMin
                                  m.MaxTemp <- item.Main.TempMax
                                  m.Humidity <- int64 item.Main.Humidity
                                  m.Pressure <- item.Main.Pressure)

    let private createWeatherItems (weathers: ForecastAPI.Weather[]) timeItemId =
        weathers
        |> Array.map (fun w -> Database.table (fun i -> i.Weathers)
                                |> Database.add (fun i -> i.ForecastTimeItemId <- timeItemId
                                                          i.Main <- w.Main
                                                          i.Description <- w.Description
                                                          i.Icon <- w.Icon))

    let private createWind (wind: ForecastAPI.Wind) timeItemId =
        Database.table (fun m -> m.Winds)
        |> Database.add (fun w -> w.ForecastTimeItemId <- timeItemId
                                  w.Speed <- wind.Speed
                                  w.Degree <- wind.Deg)
        

    let private createWeatherEntities timeItemId item: ForecastAPI.List =
        let _ = createMain item timeItemId
        let _ = createWeatherItems item.Weather timeItemId
        let _ = createWind item.Wind timeItemId
        item
    
    let fetchItemsAsync (root: ForecastAPI.Root) (forecast: ForecastEntity) = async {
        let groupedItems = root.List
                            |> Array.groupBy (fun i -> i.DtTxt.Date)
                            |> Array.map (fun (key, items) -> let itemEntity = Database.table (fun d -> d.ForecastItems)
                                                                                  |> Database.add (fun m -> m.ForecastId <- forecast.Id
                                                                                                            m.Date <- key)
                                                              (itemEntity, items))
                            |> Database.saveUpdatesAsync
                            |> Async.RunSynchronously

        groupedItems
        |> Array.map (fun (itemEntity, items) -> items
                                                 |> Array.map (fun i -> let timeItem = Database.table (fun f -> f.ForecastTimeItems)
                                                                                        |> Database.add (fun t -> t.ForecastItemId <- itemEntity.Id
                                                                                                                  t.Time <- i.DtTxt)
                                                                        (timeItem, i)))
        |> Array.collect (fun i -> i)
        |> Database.saveUpdatesAsync
        |> Async.RunSynchronously
        |> Array.map (fun (timeItem, item) -> createWeatherEntities timeItem.Id item)
        |> Database.saveUpdatesAsync
        |> Async.RunSynchronously
        |> ignore
        return forecast
    }
        
    
    let private processRequestAsync (fetch: string -> Async<ForecastAPI.Root>) lastEligibleTime city (option: ForecastEntity option) = async {
        match option with
        | Some f when f.Created >= lastEligibleTime -> return f
        | Some f -> let! root = fetch city
                    return! f
                            |> Database.update (fun i -> i.Created <- DateTimeOffset.Now.DateTime)
                            |> Database.update (fun i -> Async.RunSynchronously (Database.deleteAsync i.``main.ForecastItems by Id``) |> ignore)
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