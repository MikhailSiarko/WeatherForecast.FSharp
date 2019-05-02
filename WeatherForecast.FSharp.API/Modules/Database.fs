namespace WeatherForecast.FSharp.API.Modules

module Database =
    open FSharp.Data.Sql
    open WeatherForecast.FSharp.API.Types
    open System

    let private context = AppDbContext.GetDataContext(SelectOperations.DatabaseSide)

    let forecastQuery predicate = query {
        for forecast in context.Main.Forecasts do
            where ((%predicate) forecast)
    }
    
    let usersQuery predicate = query {
        for user in context.Main.Users do
            where ((%predicate) user)
    }
    
    let tryGetUserAsync login = async {
        return! usersQuery <@(fun u -> u.Login = login)@>
                |> Seq.tryHeadAsync
    }

    let tryGetForecastFromPredicateAsync predicate = async {
        return! forecastQuery predicate
                |> Seq.tryHeadAsync
    }

    let tryGetForecastAsync countryCode city = async {
        return! tryGetForecastFromPredicateAsync <@(fun f -> f.CountryCode = countryCode && f.Location = city)@>
    }
    
    let createUserAsync login password = async {
        let user = context.Main.Users.``Create(Login, Password)``(login, password)
        do! context.SubmitUpdatesAsync()
        return user
     }

    let private deleteItemsAsync forecastId = async {
        let! __ = Seq.``delete all items from single table`` (query {
            for i in context.Main.ForecastItems do
                where (i.ForecastId = forecastId)
        })
        return! context.SubmitUpdatesAsync()
    }

    let private createMain (item: ForecastAPI.List) entityId =
        context.Main.Mains.``Create(ForecastItemId, Humidity, MaxTemp, MinTemp, Pressure, Temp)``
            (
                entityId,
                int64 item.Main.Humidity,
                item.Main.TempMax,
                item.Main.TempMin,
                item.Main.Pressure,
                item.Main.Temp
            )

    let private createWeatherItems (weathers: ForecastAPI.Weather[]) entityId =
        weathers
        |> Array.map (fun w -> context.Main.Weathers.``Create(Description, ForecastItemId, Main)``(w.Description, entityId, w.Main))

    let private createWind (wind: ForecastAPI.Wind) entityId =
        context.Main.Winds.``Create(Degree, ForecastItemId, Speed)``(wind.Deg, entityId, wind.Speed)

    let private createWeatherEntities (item: ForecastAPI.List, entity: AppDbContext.dataContext.``main.ForecastItemsEntity``) =
        let main = createMain item entity.Id
        let weathers = createWeatherItems item.Weather entity.Id
        let wind = createWind item.Wind entity.Id
        (entity, main, weathers, wind)

    let private saveItemsAsync forecastId (items: ForecastAPI.List[]) = async {
        let tuples = items
                     |> Array.map (fun i -> let entity = context.Main.ForecastItems.``Create(Date, ForecastId)``(i.DtTxt, forecastId)
                                            (i, entity))
        do! context.SubmitUpdatesAsync()
        tuples
        |> Array.map createWeatherEntities
        |> ignore
        do! context.SubmitUpdatesAsync()
    }

    let private createItemsAsync collect (forecast: ForecastEntity) = async {
        let! items = collect forecast.CountryCode forecast.Location
        return! saveItemsAsync forecast.Id items
    }

    let updateAsync collect (forecast: ForecastEntity) = async {
        context.ClearUpdates() |> ignore
        do! deleteItemsAsync forecast.Id
        do! createItemsAsync collect forecast
        forecast.Created <- DateTimeOffset.Now.DateTime
        return! context.SubmitUpdatesAsync()
    }

    let createForecastAsync collect countryCode city = async {
        context.ClearUpdates() |> ignore
        let forecast = context.Main.Forecasts.``Create(CountryCode, Created, Location)``(countryCode, DateTimeOffset.Now.DateTime, city)
        do! context.SubmitUpdatesAsync()
        do! createItemsAsync collect forecast
        return forecast
    }

    let clearUpdates () = context.ClearUpdates() |> ignore