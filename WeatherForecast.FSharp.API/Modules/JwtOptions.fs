namespace WeatherForecast.FSharp.API.Modules

open System.Text
open Microsoft.IdentityModel.Tokens
open WeatherForecast.FSharp.API.Types

module JwtOptions =
    let private options = (Async.RunSynchronously (Settings.AsyncGetSample()))
    let Issuer = options.JwtOptions.Issuer
    let Audience = options.JwtOptions.Audience
    let Lifetime = options.JwtOptions.Lifetime
    let SymmetricSecurityKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.JwtOptions.Key))