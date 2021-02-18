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

type ValidForecast = ValidForecast of Forecast

type ExpiredForecast = ExpiredForecast of Forecast

type ForecastState =
    | Valid of ValidForecast
    | Expired of ExpiredForecast

[<Measure>]
type min

module Forecast =
    let private isValid date expTime = date >= expTime

    let validate forecast (validationDate: DateTime) (expiredAfter: float<min>) =
        let expirationTime =
            validationDate.AddMinutes(-1.0 * float expiredAfter)

        match isValid (forecast.Updated.ToUniversalTime()) expirationTime with
        | true -> Valid(ValidForecast forecast)
        | false -> Expired(ExpiredForecast forecast)

    let update (ExpiredForecast expired) (ValidForecast valid) =
        match expired.Name = valid.Name with
        | true -> ValidForecast { valid with Id = expired.Id }
        | false -> failwith "The forecast information is for another location"
