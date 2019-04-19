namespace WeatherForecast.FSharp.API.Types.Weather

open FSharp.Data

type Forecast = JsonProvider<"./Samples/data.json">

