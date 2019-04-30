namespace WeatherForecast.FSharp.API.Modules

open WeatherForecast.FSharp.API.Types
open FSharp.Data.Sql

module Mapping =
    let toMain (main: MainEntity) =
        {
            Id = main.Id;
            Temp = main.Temp;
            MinTemp = main.MinTemp;
            MaxTemp = main.MaxTemp;
            Pressure = main.Pressure;
            Humidity = main.Humidity
        }

    let toWeather (weather: WeatherEntity) =
        { Id = weather.Id; Main = weather.Main; Description = weather.Description }

    let toWind (wind: WindEntity) =
        { Id = wind.Id; Speed = wind.Speed; Degree = wind.Degree }

    let single map = Seq.headAsync >> Async.RunSynchronously >> map

    let multiple map = Seq.executeQueryAsync >> Async.RunSynchronously >> Seq.map map >> Seq.toArray

    let toForecastItem (entity: ForecastItemEntity) =
        {
            Id = entity.Id;
            ForecastId = entity.ForecastId;
            Date = entity.Date;
            Main = entity.``main.Mains by Id``
                    |> single toMain;
            Weather = entity.``main.Weathers by Id``
                        |> multiple toWeather;
            Wind = entity.``main.Winds by Id``
                    |> single toWind
        }

    let toForecast (entity: ForecastEntity) =
        {
                Id = entity.Id;
                CountryCode = entity.CountryCode;
                City = entity.Location;
                Updated = entity.Created;
                Items = entity.``main.ForecastItems by Id``
                        |> multiple toForecastItem
        }