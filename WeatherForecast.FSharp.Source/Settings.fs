namespace WeatherForecast.FSharp.Source

open FSharp.Data

type Settings = JsonProvider<"sourcesettings.json">

type ForecastSourceAPI = JsonProvider<"data.json">
