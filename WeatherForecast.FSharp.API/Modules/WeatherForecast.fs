namespace WeatherForecast.FSharp.API.Modules

open Microsoft.Extensions.Configuration
open WeatherForecast.FSharp.API.Types.Weather
open WeatherForecast.FSharp.API.Types.Application

type WeatherForecast (configuration: IConfiguration) =
    let apiKey = configuration.GetSection("WeatherForecastServiceApiKey").Value
    let apiUrl = Printf.StringFormat<string -> string -> string -> string, _> "http://api.openweathermap.org/data/2.5/forecast?q=%s,%s&units=metric&appid=%s"

    let mapMain (main: Forecast.Main): Main =
        { Temp = main.Temp; MinTemp = main.TempMin; MaxTemp = main.TempMax; Pressure = main.Pressure; Humidity = main.Humidity }

    let mapWeather (weather: Forecast.Weather): Weather =
        { Id = weather.Id; Main = weather.Main; Description = weather.Description }

    let mapWind (wind: Forecast.Wind): Wind =
        { Speed = wind.Speed; Degree = wind.Deg }

    let mapList (data: Forecast.List): WeatherItem =
        { Date = data.DtTxt; Main = mapMain data.Main; Weather = Array.map mapWeather data.Weather; Wind = mapWind data.Wind }


    member __.LoadAsync countryCode city = async {
        let! result = Forecast.AsyncLoad (sprintf apiUrl city countryCode apiKey)
        return Array.map mapList result.List
    }