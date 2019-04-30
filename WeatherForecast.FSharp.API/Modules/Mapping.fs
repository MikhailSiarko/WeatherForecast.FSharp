namespace WeatherForecast.FSharp.API.Modules

open WeatherForecast.FSharp.API.Types
open FSharp.Data.Sql

module Mapping =
    let main (main: MainEntity) =
        {
            Id = main.Id;
            Temp = main.Temp;
            MinTemp = main.MinTemp;
            MaxTemp = main.MaxTemp;
            Pressure = main.Pressure;
            Humidity = main.Humidity
        }

    let weather (weather: WeatherEntity) =
        { Id = weather.Id; Main = weather.Main; Description = weather.Description }

    let wind (wind: WindEntity) =
        { Id = wind.Id; Speed = wind.Speed; Degree = wind.Degree }

    let single map source =
        source
        |> Seq.headAsync
        |> Async.RunSynchronously
        |> map

    let multiple map source =
        source
        |> Seq.executeQueryAsync
        |> Async.RunSynchronously
        |> Seq.map map
        |> Seq.toArray

    let forecastItem (entity: ForecastItemEntity) =
        {
            Id = entity.Id;
            ForecastId = entity.ForecastId;
            Date = entity.Date;
            Main = entity.``main.Mains by Id``
                    |> single main;
            Weather = entity.``main.Weathers by Id``
                        |> multiple weather;
            Wind = entity.``main.Winds by Id``
                    |> single wind
        }

    let forecast (entity: ForecastEntity) =
        {
                Id = entity.Id;
                CountryCode = entity.CountryCode;
                City = entity.Location;
                Updated = entity.Created;
                Items = entity.``main.ForecastItems by Id``
                        |> multiple forecastItem
        }