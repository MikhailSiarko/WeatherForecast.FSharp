namespace WeatherForecast.FSharp.API.Modules

open System.Text
open Microsoft.IdentityModel.Tokens

module JwtOptions =
    let private key = "AuthenticationKey"
    let Issuer = "ParcelTracker_API"
    let Audience = "ParcelTracker_Client"
    let Lifetime = 20
    let SymmetricSecurityKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))