namespace WeatherForecast.FSharp.API.Modules

open Microsoft.Extensions.Configuration
open WeatherForecast.FSharp.API.Types

type WeatherForecast (configuration: IConfiguration) =
    let apiKey = configuration.GetSection("WeatherForecastServiceApiKey").Value
    let apiUrl = Printf.StringFormat<string -> string -> string -> string, _> "http://api.openweathermap.org/data/2.5/forecast?q=%s,%s&units=metric&appid=%s"

    member __.ForecastAsync countryCode city = async {
        return! Forecast.AsyncLoad (sprintf apiUrl city countryCode apiKey)
    }