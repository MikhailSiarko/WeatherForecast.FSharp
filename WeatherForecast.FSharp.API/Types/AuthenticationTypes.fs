namespace WeatherForecast.FSharp.API.Types.Authentication

open System.Text
open Microsoft.IdentityModel.Tokens
open WeatherForecast.FSharp.API.Types.Application

module JwtOptions =
    let private key = "AuthenticationKey"
    let Issuer = "ParcelTracker_API"
    let Audience = "ParcelTracker_Client"
    let Lifetime = 20
    let SymmetricSecurityKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))

type AuthenticationData =
    { Token: string; User: User }
    static member Create user token = { User = user; Token = token }