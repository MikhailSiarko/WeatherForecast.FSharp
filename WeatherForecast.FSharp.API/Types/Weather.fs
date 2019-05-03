namespace WeatherForecast.FSharp.API.Types

open FSharp.Data
open Microsoft.Extensions.Configuration

type ForecastAPI = JsonProvider<"./Samples/data.json">

type WeatherAPIService (configuration: IConfiguration) =
    let apiKey = configuration.GetSection("WeatherForecastServiceApiKey").Value
    let apiUrl = Printf.StringFormat<string -> string -> string, _> "http://api.openweathermap.org/data/2.5/forecast?q=%s&units=metric&appid=%s"

    member __.Load city = async {
        let! weather = ForecastAPI.AsyncLoad (sprintf apiUrl city apiKey)
        return weather
    }
