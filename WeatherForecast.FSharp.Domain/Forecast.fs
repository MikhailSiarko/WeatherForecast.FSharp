namespace WeatherForecast.FSharp.Domain

open System

type Main =
    { Id: int64
      ForecastTimeItemId: int64
      Temp: decimal
      MinTemp: decimal
      MaxTemp: decimal
      Pressure: decimal
      Humidity: int64 }

type Weather =
    { Id: int64
      ForecastTimeItemId: int64
      Main: string
      Description: string
      Icon: string }

type Wind =
    { Id: int64
      ForecastTimeItemId: int64
      Speed: decimal
      Degree: decimal }

type ForecastTimeItem =
    { Id: int64
      ForecastItemId: int64
      Time: DateTime
      Main: Main
      Weather: Weather
      Wind: Wind }

type ForecastItem =
    { Id: int64
      ForecastId: int64
      Date: DateTime
      TimeItems: ForecastTimeItem [] }

type Forecast =
    { Id: int64
      Country: string
      Name: string
      Updated: DateTime
      Items: ForecastItem [] }
type ForecastState =
    | Valid
    | Expired

[<Measure>]
type min

module Forecast =
    let private isValid date expTime = date >= expTime

    let validate lastUpdateDate expirationDate =
        match isValid lastUpdateDate expirationDate with
        | true -> Valid
        | false -> Expired
