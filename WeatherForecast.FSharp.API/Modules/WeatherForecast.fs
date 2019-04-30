namespace WeatherForecast.FSharp.API.Modules

open System
open Microsoft.Extensions.Configuration
open WeatherForecast.FSharp.API.Types.Weather
open WeatherForecast.FSharp.API.Types.Database
open FSharp.Data.Sql

type WeatherForecast (configuration: IConfiguration) =
    let apiKey = configuration.GetSection("WeatherForecastServiceApiKey").Value
    let apiUrl = Printf.StringFormat<string -> string -> string -> string, _> "http://api.openweathermap.org/data/2.5/forecast?q=%s,%s&units=metric&appid=%s"

    let context = AppDbContext.GetDataContext(SelectOperations.DatabaseSide)

    let forecastQuery predicate = query {
        for forecast in context.Main.Forecasts do
            where ((%predicate) forecast)
    }

    let itemsQuery forecastId = query {
        for i in context.Main.ForecastItems do
            where (i.ForecastId = forecastId)
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

    let update forecastId = async {
        let! forecast = tryGetForecastFromPredicateAsync <@(fun i -> i.Id = forecastId)@>
        match forecast with
        | Some f -> f.Created <- DateTimeOffset.Now.DateTime
                    return! context.SubmitUpdatesAsync()
        | None -> return ()
    }

    let collectItems countryCode city = async {
        let! weather = ForecastAPI.AsyncLoad (sprintf apiUrl city countryCode apiKey)
        return weather.List
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
        let entities = tuples
                        |> Array.map createWeatherEntities
        do! context.SubmitUpdatesAsync()
    }

    let createForecast countryCode city = async {
        let forecast = context.Main.Forecasts.``Create(CountryCode, Created, Location)``(countryCode, DateTimeOffset.Now.DateTime, city)
        do! context.SubmitUpdatesAsync()
        return forecast
    }

    let createItemsAsync countryCode city forecastId = async {
        let! items = collectItems countryCode city
        return! saveItemsAsync forecastId items
    }

    member __.LoadAsync countryCode city = async {
        let lastEligibleTime = DateTimeOffset.Now.AddMinutes(-20.0).DateTime
        let! forecast = tryGetForecastAsync countryCode city
        match forecast with
        | Some f when f.Created >= lastEligibleTime -> return Mapping.forecast f
        | Some f -> context.ClearUpdates() |> ignore
                    do! deleteItemsAsync f.Id
                    let! items = createItemsAsync countryCode city f.Id
                    do! update f.Id
                    return Mapping.forecast f
        | None -> context.ClearUpdates() |> ignore
                  let! forecast = createForecast countryCode city
                  do! createItemsAsync countryCode city forecast.Id
                  return Mapping.forecast forecast
    }