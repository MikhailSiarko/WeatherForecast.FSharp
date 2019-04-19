namespace WeatherForecast.FSharp.API.Types.Authentication

open WeatherForecast.FSharp.API.Types.Application

type AuthenticationData =
    { Token: string; User: User }
    static member Create user token = { User = user; Token = token }