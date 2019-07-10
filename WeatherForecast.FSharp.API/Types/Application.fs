namespace WeatherForecast.FSharp.API.Types

open System
open FSharp.Data

type Main () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val Temp = Unchecked.defaultof<decimal> with get, set
    member val MinTemp = Unchecked.defaultof<decimal> with get, set
    member val MaxTemp = Unchecked.defaultof<decimal> with get, set
    member val Pressure = Unchecked.defaultof<decimal> with get, set
    member val Humidity = Unchecked.defaultof<int64> with get, set

type Weather () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val Main = Unchecked.defaultof<string> with get, set
    member val Description = Unchecked.defaultof<string> with get, set
    member val Icon = Unchecked.defaultof<string> with get, set

type Wind () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val Speed = Unchecked.defaultof<decimal> with get, set
    member val Degree = Unchecked.defaultof<decimal> with get, set

type LoginData = { Login: string; Password: string }

type RegisterData = { Login: string; Password: string; ConfirmPassword: string }

type User () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val Login = Unchecked.defaultof<string> with get, set

type ForecastTimeItem () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val ForecastItemId = Unchecked.defaultof<int64> with get, set
    member val Time = Unchecked.defaultof<DateTime> with get, set
    member val Main = Unchecked.defaultof<Main> with get, set
    member val Weather = Unchecked.defaultof<Weather> with get, set
    member val Wind = Unchecked.defaultof<Wind> with get, set

type ForecastItem () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val ForecastId = Unchecked.defaultof<int64> with get, set
    member val Date = Unchecked.defaultof<DateTime> with get, set
    member val TimeItems = Unchecked.defaultof<ForecastTimeItem[]> with get, set

type Forecast () =
    member val Id = Unchecked.defaultof<int64> with get, set
    member val CountryCode = Unchecked.defaultof<string> with get, set
    member val City = Unchecked.defaultof<string> with get, set
    member val Updated = Unchecked.defaultof<DateTime> with get, set
    member val Items = Unchecked.defaultof<ForecastItem[]> with get, set

type AuthenticationData =
    { Token: string; User: User }
    static member Create user token = { User = user; Token = token }

type Settings = JsonProvider<"appsettings.json">