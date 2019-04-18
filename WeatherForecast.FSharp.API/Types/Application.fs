namespace WeatherForecast.FSharp.API.Types.Application

open System

type Parcel = { Id: Guid; TrackingNumber: string; Status: string; CarrierCode: string; LastUpdateTime: DateTime }

type LoginData = { Login: string; Password: string }

type RegisterData = { Login: string; Password: string; ConfirmPassword: string }

type User = { Id: int64; Login: string }