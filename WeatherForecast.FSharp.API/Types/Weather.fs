namespace WeatherForecast.FSharp.API.Types.Weather

open FSharp.Data

type ForecastAPI = JsonProvider<"./Samples/data.json">

