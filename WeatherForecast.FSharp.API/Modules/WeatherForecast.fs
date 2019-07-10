namespace WeatherForecast.FSharp.API.Modules

open AutoMapper
open System
open System.Linq
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

module WeatherForecast =
    let private settings = Settings.GetSample()
    
    let private createMain (item: ForecastAPI.List) timeItemId =
        Database.table (fun i -> i.Mains)
        |> Database.add (fun m -> m.ForecastTimeItemId <- timeItemId
                                  m.Temp <- item.Main.Temp
                                  m.MinTemp <- item.Main.TempMin
                                  m.MaxTemp <- item.Main.TempMax
                                  m.Humidity <- int64 item.Main.Humidity
                                  m.Pressure <- item.Main.Pressure)

    let private createWeather (weather: ForecastAPI.Weather) timeItemId =
        Database.table (fun i -> i.Weathers)
        |> Database.add (fun i -> i.ForecastTimeItemId <- timeItemId
                                  i.Main <- weather.Main
                                  i.Description <- weather.Description
                                  i.Icon <- sprintf (Printf.StringFormat<_> settings.IconUrlFormat) weather.Icon)

    let private createWind (wind: ForecastAPI.Wind) timeItemId =
        Database.table (fun m -> m.Winds)
        |> Database.add (fun w -> w.ForecastTimeItemId <- timeItemId
                                  w.Speed <- wind.Speed
                                  w.Degree <- wind.Deg)

    let private createWeatherEntitiesAsync (timeItem: ForecastTimeItem) (item: ForecastAPI.List) = async {
        let main = createMain item timeItem.Id
        let weather = createWeather (Array.head item.Weather) timeItem.Id
        let wind = createWind item.Wind timeItem.Id
        do! Database.saveUpdatesAsync ()
        return (main, weather, wind)
    }
        
    let private createForecastTimeItem date itemId =
        Database.table (fun f -> f.ForecastTimeItems)
        |> Database.add (fun t -> t.ForecastItemId <- itemId
                                  t.Time <- date)
        
    let private createForecastItem date forecastId =
        Database.table (fun d -> d.ForecastItems)
        |> Database.add (fun m -> m.ForecastId <- forecastId
                                  m.Date <- date)
        
    let private mapGroupedForecastItem forecastId (date, items) =
        let itemEntity = createForecastItem date forecastId
        (itemEntity, items)
        
    let private mapForecastItemInTuple (itemEntity, items) = AutoMap.mapTo<ForecastItem> itemEntity, items
    
    let private mapForecastItemToTimeItem itemId (apiItem: ForecastAPI.List) =
        let timeItemEntity = createForecastTimeItem apiItem.DtTxt itemId
        (timeItemEntity, apiItem)
        
    let private mapForecastItemTupleToTimeItems (item: ForecastItem, apiItems) =
        let timeItemEntities = apiItems
                                |> Array.map (mapForecastItemToTimeItem item.Id)
        (item, timeItemEntities)
        
    let private mapTimeItemEntityAsync (entity, item) = async {
        let timeItem = (AutoMap.mapTo entity)
        let! (main, weather, wind) = createWeatherEntitiesAsync timeItem item
        timeItem.Main <- main |> AutoMap.mapTo
        timeItem.Weather <- AutoMap.mapTo weather
        timeItem.Wind <- wind |> AutoMap.mapTo
        return timeItem
    }
    
    let private mapItemAsync (item: ForecastItem, timeItemEntities) = async {
        let! timeItems = timeItemEntities
                        |> Seq.map mapTimeItemEntityAsync
                        |> Async.Parallel

        item.TimeItems <- timeItems
        return item
    }
    
    let saveItemsAsync (root: ForecastAPI.Root) (forecast: Forecast) = async {
        let itemTuples = root.List
                            |> Array.groupBy (fun i -> i.DtTxt.Date)
                            |> Array.map (mapGroupedForecastItem forecast.Id)
                            |> Database.saveUpdatesAsync
                            |> Async.RunSynchronously
                            |> Array.map mapForecastItemInTuple

        let! items = itemTuples
                    |> Array.map mapForecastItemTupleToTimeItems
                    |> Database.saveUpdatesAsync
                    |> Async.RunSynchronously
                    |> Array.map mapItemAsync
                    |> Async.Parallel
                    
        forecast.Items <- items
        return forecast
    }
    
    let private getTimeItems (query: IQueryable<ForecastTimeItemEntity>) =
        let timeItemEntities = query
                               |> Database.executeAsync
                               |> Async.RunSynchronously
                               
        timeItemEntities
        |> Seq.map (fun t -> let timeItem = AutoMap.mapTo<ForecastTimeItem> t
                             timeItem.Main <- Database.single t.``main.Mains by Id`` AutoMap.mapTo                                                                                                         
                             timeItem.Wind <- Database.single t.``main.Winds by Id`` AutoMap.mapTo                                                                                                                           
                             timeItem.Weather <- Database.single t.``main.Weathers by Id`` AutoMap.mapTo
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