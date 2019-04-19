namespace WeatherForecast.FSharp.API.Types

open FSharp.Data

type Forecast = JsonProvider<"./Samples/data.json">

