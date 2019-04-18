namespace WeatherForecast.FSharp.API.Types

open FSharp.Data

type Forecast = JsonProvider<"http://api.openweathermap.org/data/2.5/forecast?q=London,UK&units=metric&appid=24fac60176b052f24f85e44fe1563482">

