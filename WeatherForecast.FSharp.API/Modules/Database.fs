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

    let tryGetForecastFromPredicateAsync predicate = async {
        return! forecastQuery predicate
                |> Seq.tryHeadAsync
    }

    let tryGetForecastAsync countryCode city = async {
        return! tryGetForecastFromPredicateAsync <@(fun f -> f.CountryCode = countryCode && f.Location = city)@>
    }

    let deleteItemsAsync forecastId = async {
        let! count = Seq.``delete all items from single table`` (query {
            for i in context.Main.ForecastItems do
                where (i.ForecastId = forecastId)
        })
        return! context.SubmitUpdatesAsync()
    }

    let update (forecast: ForecastEntity) = async {
        forecast.Created <- DateTimeOffset.Now.DateTime
        return! context.SubmitUpdatesAsync()
    }

    let createMain (item: ForecastAPI.List) entityId =
        context.Main.Mains.``Create(ForecastItemId, Humidity, MaxTemp, MinTemp, Pressure, Temp)``
            (
                entityId,
                int64 item.Main.Humidity,
                item.Main.TempMax,
                item.Main.TempMin,
                item.Main.Pressure,
                item.Main.Temp
            )

    let createWeatherItems (weathers: ForecastAPI.Weather[]) entityId =
        weathers
        |> Array.map (fun w -> context.Main.Weathers.``Create(Description, ForecastItemId, Main)``(w.Description, entityId, w.Main))

    let createWind (wind: ForecastAPI.Wind) entityId =
        context.Main.Winds.``Create(Degree, ForecastItemId, Speed)``(wind.Deg, entityId, wind.Speed)

    let createWeatherEntities (item: ForecastAPI.List, entity: AppDbContext.dataContext.``main.ForecastItemsEntity``) =
        let main = createMain item entity.Id
        let weathers = createWeatherItems item.Weather entity.Id
        let wind = createWind item.Wind entity.Id
        (entity, main, weathers, wind)

    let saveItemsAsync forecastId (items: ForecastAPI.List[]) = async {
        let tuples = items
                     |> Array.map (fun i -> let entity = context.Main.ForecastItems.``Create(Date, ForecastId)``(i.DtTxt, forecastId)
                                            (i, entity))
        do! context.SubmitUpdatesAsync()
        tuples
        |> Array.map createWeatherEntities
        |> ignore
        do! context.SubmitUpdatesAsync()
    }

    let createForecast countryCode city = async {
        let forecast = context.Main.Forecasts.``Create(CountryCode, Created, Location)``(countryCode, DateTimeOffset.Now.DateTime, city)
        do! context.SubmitUpdatesAsync()
        return forecast
    }

    let createItemsAsync collect countryCode city forecastId = async {
        let! items = collect countryCode city
        return! saveItemsAsync forecastId items
    }

    let clearUpdates () = context.ClearUpdates() |> ignore