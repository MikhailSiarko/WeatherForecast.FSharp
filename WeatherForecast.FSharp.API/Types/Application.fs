namespace WeatherForecast.FSharp.API.Types.Application

open System

type Main = { Temp: decimal; MinTemp: decimal; MaxTemp: decimal; Pressure: decimal; Humidity: int }

type Weather = { Id: int; Main: string; Description: string }

type Wind = { Speed: decimal; Degree: decimal }

type WeatherItem = { Date: DateTime; Main: Main; Weather: Weather[]; Wind: Wind }

type LoginData = { Login: string; Password: string }

type RegisterData = { Login: string; Password: string; ConfirmPassword: string }

type User = { Id: int64; Login: string }