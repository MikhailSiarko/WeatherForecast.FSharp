namespace WeatherForecast.FSharp.Authentication

open System.Text
open Microsoft.IdentityModel.Tokens
open FSharp.Data

type internal Settings = JsonProvider<"authenticationsettings.json">

module JwtOptions =
    let private options = Settings.GetSample()
    let Issuer = options.Issuer
    let Audience = options.Audience
    let Lifetime = options.Lifetime
    let SymmetricSecurityKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Key))