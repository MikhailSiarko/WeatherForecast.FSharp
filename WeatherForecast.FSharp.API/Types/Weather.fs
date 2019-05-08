namespace WeatherForecast.FSharp.API.Types

open FSharp.Data

type ForecastAPI = JsonProvider<"./Samples/data.json">

type GetForecast = string -> Async<Forecast>

module WeatherAPI =
    let configureLoad apiUrl apiKey city = async {
        let! weather = ForecastAPI.AsyncLoad (sprintf apiUrl city apiKey)
        return weather
    }
