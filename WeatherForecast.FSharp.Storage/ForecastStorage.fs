module ForecastStorage
    open System.Linq
    open WeatherForecast.FSharp.Storage
    open WeatherForecast.FSharp.Domain
    open FSharp.Data.Sql
    open System

    type internal ForecastEntity = AppDbContext.dataContext.``main.ForecastsEntity``
    
    type internal ForecastItemEntity = AppDbContext.dataContext.``main.ForecastItemsEntity``
    
    type internal ForecastTimeItemEntity = AppDbContext.dataContext.``main.ForecastTimeItemsEntity``
    
    type internal MainEntity = AppDbContext.dataContext.``main.MainsEntity``
    
    type internal WeatherEntity = AppDbContext.dataContext.``main.WeathersEntity``
    
    type internal WindEntity = AppDbContext.dataContext.``main.WindsEntity``

    let private forecastLocationPredicate (location: string) (forecastEntity: ForecastEntity) =
        forecastEntity.Location.ToLower() = location.ToLower()
    
    let private mapTimeItem (t: ForecastTimeItemEntity) =
        {
            Id = t.Id;
            ForecastItemId = t.ForecastItemId;
            Time = t.Time;
            Main = t.``main.Mains by Id``
                    |> Seq.map (fun m -> m.MapTo<Main>())
                    |> Seq.head;
            Weather = t.``main.Weathers by Id``
                        |> Seq.map (fun w -> w.MapTo<Weather>())
                        |> Seq.head;
            Wind = t.``main.Winds by Id``
                    |> Seq.map (fun w -> w.MapTo<Wind>())
                    |> Seq.head
        }

    let private mapItem (item: ForecastItemEntity) =
        {
            Id = item.Id;
            ForecastId = item.ForecastId;
            Date = item.Date;
            TimeItems = item.``main.ForecastTimeItems by Id``
                        |> Seq.toArray
                        |> Array.map mapTimeItem
        }
        
    let private mapForecast (f: ForecastEntity) =
        {
            Id = f.Id;
            CountryCode = f.CountryCode;
            City = f.Location;
            Updated = f.Created;
            Items = f.``main.ForecastItems by Id``
                    |> Seq.toArray
                    |> Array.map mapItem
        }
    
    let private fillItem item (entity: ForecastItemEntity) =
        entity.Date <- item.Date
        entity.ForecastId <- item.ForecastId
    
    let private fillTimeItem timeItem (entity: ForecastTimeItemEntity) =
        entity.Time <- timeItem.Time
        entity.ForecastItemId <- timeItem.ForecastItemId
    
    let private fillForecast forecast (entity: ForecastEntity) =
        entity.Location <- forecast.City
        entity.CountryCode <- forecast.CountryCode
        entity.Created <- DateTimeOffset.Now.DateTime
    
    let private fillMain main (entity: MainEntity) =
        entity.Humidity <- main.Humidity
        entity.Pressure <- main.Pressure
        entity.Temp <- main.Temp
        entity.MaxTemp <- main.MaxTemp
        entity.MinTemp <- main.MinTemp
        entity.ForecastTimeItemId <- main.ForecastTimeItemId
    
    let private fillWeather weather (entity: WeatherEntity) =
        entity.Description <- weather.Description
        entity.Icon <- weather.Icon
        entity.Main <- weather.Main
        entity.ForecastTimeItemId <- weather.ForecastTimeItemId
    
    let private fillWind wind (entity: WindEntity) =
        entity.Degree <- wind.Degree
        entity.Speed <- wind.Speed
        entity.ForecastTimeItemId <- wind.ForecastTimeItemId

    let inline private saveSingle selector fillEntity updateRecord record = async {
        let entity = Database.table selector
                     |> Database.add (fillEntity record)
        do! Database.saveUpdatesAsync()
        return updateRecord record entity
    }
    
    let inline private saveMultiple selector fillEntity updateRecord records = async {
        let tuples = records
                      |> Array.map (fun t -> let entity = Database.table selector
                                                          |> Database.add (fillEntity t)
                                             (t, entity))
        do! Database.saveUpdatesAsync()
        return tuples
                |> Array.map updateRecord
    }
    
    let private updateForecast r (e: ForecastEntity) =
        { r with Id = e.Id; Items = Array.map (fun i -> { i with ForecastId = e.Id }) r.Items }
        
    let private updateForecastItem (i, e: ForecastItemEntity) =
        { i with Id = e.Id; TimeItems = Array.map (fun t -> { t with ForecastItemId = e.Id }) i.TimeItems }
        
    let private updateForecastTimeItem (t, e: ForecastTimeItemEntity) =
        { t with Id = e.Id; Main = { t.Main with ForecastTimeItemId = e.Id }; Weather = { t.Weather with ForecastTimeItemId = e.Id }; Wind = { t.Wind with ForecastTimeItemId = e.Id } }
        
    let private updateMain (r: Main) (e: MainEntity) = { r with Id = e.Id }
    
    let private updateWeather (r: Weather) (e: WeatherEntity) = { r with Id = e.Id }
    
    let private updateWind (r: Wind) (e: WindEntity) = { r with Id = e.Id }
    
    let private saveTimeItemDetailsAsync t = async {
        let! main = t.Main
                    |> saveSingle (fun c -> c.Mains) fillMain updateMain
        let! weather = t.Weather
                       |> saveSingle (fun c -> c.Weathers) fillWeather updateWeather
        let! wind = t.Wind
                    |> saveSingle (fun c -> c.Winds) fillWind updateWind
        return { t with Main = main; Weather = weather; Wind = wind }
    }
    
    let private saveTimeItemsAsync i = async {
        let! timeItems = i.TimeItems
                         |> saveMultiple (fun c -> c.ForecastTimeItems) fillTimeItem updateForecastTimeItem

        let! timeItems = timeItems
                         |> Array.map saveTimeItemDetailsAsync
                         |> Async.Parallel
                         

        return { i with TimeItems = timeItems }
    }
    
    let private saveItemsAsync items = async {
        let! items = items
                     |> saveMultiple (fun c -> c.ForecastItems) fillItem updateForecastItem
                     
        return! items
                |> Array.map saveTimeItemsAsync
                |> Async.Parallel
    }
    
    let private saveNewForecastAsync (newEntity: NewEntity<Forecast>) = async {
        let! f = newEntity.Value
                 |> saveSingle (fun c -> c.Forecasts) fillForecast updateForecast
                 
        let! items = saveItemsAsync f.Items
                     
        return { f with Items = items }
    }
        
    let private updateExistingForecastAsync (existingEntity: ExistingEntity<Forecast>) = async {
        let forecast = existingEntity.Value
        let! items = forecast.Items
                    |> Array.map (fun i -> { i with ForecastId = forecast.Id })
                    |> saveItemsAsync
                     
        let now = DateTime.UtcNow
        let! entity =  Database.singleAsync
                            <@ fun c -> c.Forecasts :> IQueryable<_> @>
                                <@ fun f -> f.Location.ToLower() = forecast.City.ToLower() @>
        entity.Created <- now
        do! Database.saveUpdatesAsync()
        return { existingEntity.Value with Items = items; Updated = now }
    }
    
    let private deleteForecastItemsAsync forecastId = async {
        let! _ = Database.queryTo <@ fun c -> c.ForecastItems :> IQueryable<_> @> <@ fun i -> i.ForecastId = forecastId @>
                 |> Seq.``delete all items from single table``
        return ()
    }
    
    let tryGetAsync (location: string) = async {
        let! option =
            Database.trySingleAsync
                <@ fun m -> m.Forecasts :> IQueryable<_> @>
                    <@ fun f -> f.Location.ToLower() = location.ToLower() @>
        
        return match option with
                | Some f -> f
                             |> mapForecast
                             |> Some
                | None -> None
    }
    
    let saveAsync (ValidForecast forecast) = async {
        return! match Database.exists (fun c -> c.Forecasts :> IQueryable<_>) (fun f -> forecastLocationPredicate f.City) forecast with
                | Exists f ->
                    deleteForecastItemsAsync f.Value.Id
                    |> Async.RunSynchronously
                    |> ignore
                    |> Database.saveUpdatesAsync
                    |> Async.RunSynchronously
                    updateExistingForecastAsync f
                | New f -> saveNewForecastAsync f
    }

