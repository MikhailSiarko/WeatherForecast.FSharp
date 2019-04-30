namespace WeatherForecast.FSharp.API.Types

open System

type Main = { Id: int64; Temp: decimal; MinTemp: decimal; MaxTemp: decimal; Pressure: decimal; Humidity: int64 }

type Weather = { Id: int64; Main: string; Description: string }

type Wind = { Id: int64; Speed: decimal; Degree: decimal }

type LoginData = { Login: string; Password: string }

type RegisterData = { Login: string; Password: string; ConfirmPassword: string }

type User = { Id: int64; Login: string }

type ForecastItem = { Id: int64; ForecastId: int64; Date: DateTime; Main: Main; Weather: Weather[]; Wind: Wind }

type Forecast = { Id: int64; CountryCode: string; City: string; Updated: DateTime; Items: ForecastItem[] }

type AuthenticationData =
    { Token: string; User: User }
    static member Create user token = { User = user; Token = token }