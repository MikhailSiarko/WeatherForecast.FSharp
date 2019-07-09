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

    let private createWeatherEntities (timeItem: ForecastTimeItem) (item: ForecastAPI.List) =
        let main = createMain item timeItem.Id
        let weathers = createWeatherItems item.Weather timeItem.Id
        let wind = createWind item.Wind timeItem.Id
        (Async.RunSynchronously (Database.saveUpdatesAsync ())) |> ignore
        timeItem.Main <- main |> AutoMap.mapTo
        timeItem.Weathers <- weathers |> Array.map AutoMap.mapTo
        timeItem.Wind <- wind |> AutoMap.mapTo
        timeItem
        
    let private createForecastTimeItem date itemId =
        Database.table (fun f -> f.ForecastTimeItems)
        |> Database.add (fun t -> t.ForecastItemId <- itemId
                                  t.Time <- date)
        
    let private createForecastItem date forecastId =
        Database.table (fun d -> d.ForecastItems)
        |> Database.add (fun m -> m.ForecastId <- forecastId
                                  m.Date <- date)
    
    let saveItemsAsync (root: ForecastAPI.Root) (forecast: Forecast) = async {
        let groupedItems = root.List
                            |> Array.groupBy (fun i -> i.DtTxt.Date)
                            |> Array.map (fun (key, items) -> let itemEntity = createForecastItem key forecast.Id
                                                              (itemEntity, items))
                            |> Database.saveUpdatesAsync
                            |> Async.RunSynchronously
                            |> Array.map (fun (itemEntity, items) -> (AutoMap.mapTo<ForecastItem> itemEntity, items))

        let items = groupedItems
                        |> Array.map (fun (item, items) -> let timeItemEntities = items
                                                                                  |> Array.map (fun i -> let timeItemEntity = createForecastTimeItem i.DtTxt item.Id
                                                                                                         (timeItemEntity, i))
                                                           (item, timeItemEntities))
                        |> Database.saveUpdatesAsync
                        |> Async.RunSynchronously
                        |> Array.map (fun (item, timeItemEntities) -> let timeItems = timeItemEntities
                                                                                      |> Seq.map (fun (timeItemEntity, i) -> createWeatherEntities (AutoMap.mapTo timeItemEntity) i)
                                                                                      |> Seq.map AutoMap.mapTo
                                                                                      |> Seq.toArray
                                                                      item.TimeItems <- timeItems
                                                                      item)
        forecast.Items <- items
        return forecast
    }
    
    let private getTimeItems (query: IQueryable<ForecastTimeItemEntity>) =
        let timeItemEntities = query
                               |>Database.executeAsync
                               |> Async.RunSynchronously
                               
        timeItemEntities
        |> Seq.map (fun t -> let timeItem = AutoMap.mapTo<ForecastTimeItem> t
                             timeItem.Main <- Database.single t.``main.Mains by Id`` AutoMap.mapTo                                                                                                         
                             timeItem.Wind <- Database.single t.``main.Winds by Id`` AutoMap.mapTo                                                                                                                           
                             timeItem.Weathers <- Database.many t.``main.Weathers by Id`` AutoMap.mapTo
                             timeItem)
        |> Seq.sortBy (fun k -> k.Time)
        |> Seq.toArray
    
    let private getItems (query: IQueryable<ForecastItemEntity>) =
        query
        |> Database.executeAsync
        |> Async.RunSynchronously
        |> Seq.map (fun i -> let timeItems = getTimeItems i.``main.ForecastTimeItems by Id``
                             let item = AutoMap.mapTo<ForecastItem> i
                             item.TimeItems <- timeItems
                             item)
        |> Seq.sortBy (fun v -> v.Date)
        |> Seq.toArray
    
    let private processRequestAsync (fetch: string -> Async<ForecastAPI.Root>) lastEligibleTime city (option: ForecastEntity option) = async {
        match option with
        | Some f when f.Created >= lastEligibleTime -> let forecast = AutoMap.mapTo<Forecast> f
                                                       forecast.Items <- getItems f.``main.ForecastItems by Id``
                                                       return forecast
                                                       
        | Some f -> let! root = fetch city
                    return! f
                            |> Database.update (fun i -> i.Created <- DateTimeOffset.Now.DateTime)
                            |> Database.update (fun i -> Async.RunSynchronously (Database.deleteAsync i.``main.ForecastItems by Id``) |> ignore)
                            |> Database.saveUpdatesAsync
                            |> Async.RunSynchronously
                            |> AutoMap.mapTo
                            |> saveItemsAsync root
        | None -> let! root = fetch city
                  return! Database.table (fun i -> i.Forecasts)
                          |> Database.add (fun f -> f.Location <- root.City.Name
                                                    f.CountryCode <- root.City.Country
                                                    f.Created <- DateTimeOffset.Now.DateTime)
                          |> Database.saveUpdatesAsync
                          |> Async.RunSynchronously
                          |> AutoMap.mapTo
                          |> saveItemsAsync root
    }

    let loadAsync expirationTime fetch (city: string) = async {
        let lastEligibleTime = DateTimeOffset.Now.AddMinutes(-1.0 * expirationTime).DateTime
        let! forecastOption = Database.queryTo <@ fun m -> m.Forecasts :> IQueryable<_> @>
                              |> Database.where <@ fun f -> f.Location.ToLower() = city.ToLower() @>
                              |> Database.singleOrDefaultAsync
        return forecastOption
                |> processRequestAsync fetch lastEligibleTime city
                |> Async.RunSynchronously
    }