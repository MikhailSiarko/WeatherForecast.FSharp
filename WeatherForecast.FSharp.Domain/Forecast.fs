namespace WeatherForecast.FSharp.Domain
open System

type Main = {
    Id: int64;
    ForecastTimeItemId: int64;
    Temp: decimal;
    MinTemp: decimal;
    MaxTemp: decimal;
    Pressure: decimal;
    Humidity: int64
}

type Weather = {
    Id: int64;
    ForecastTimeItemId: int64;
    Main: string;
    Description: string;
    Icon: string
}

type Wind = {
    Id: int64;
    ForecastTimeItemId: int64;
    Speed: decimal;
    Degree: decimal
}

type ForecastTimeItem = {
    Id: int64;
    ForecastItemId: int64
    Time: DateTime;
    Main: Main;
    Weather: Weather
    Wind: Wind
}

type ForecastItem = {
    Id: int64;
    ForecastId: int64;
    Date: DateTime
    TimeItems: ForecastTimeItem[]
}

type Forecast = {
    Id: int64;
    CountryCode: string;
    City: string;
    Updated: DateTime;
    Items: ForecastItem[]
}

type ExpiredForecast = { Forecast: Forecast; ExpiredAt: DateTime }

type ValidForecast = { Forecast: Forecast }

type ForecastState = Valid of ValidForecast | Expired of ExpiredForecast

module Forecast =
    let private isValid date expTime = date >= expTime
    
    let validate (f, expInterval) =
        let expirationTime = DateTimeOffset.Now.AddMinutes(-1.0 * expInterval).DateTime
        match isValid f.Updated expirationTime with
        | true -> Valid({ Forecast = f })
        | false -> Expired({ Forecast = f; ExpiredAt = expirationTime })