module ForecastTests
    open System
    open WeatherForecast.FSharp.Domain
    open Xunit
    let testForecast1 = { Id = 1L; CountryCode = "BY"; City = "Minsk"; Updated = DateTime.UtcNow; Items = [||] }

    [<Fact>]
    let ``Validate Forecast. Given: Forecast Date = Now - 21 min; ExpirationTime = 20 min. ExpectedResult: Expired`` () =
        match Forecast.validate ({ testForecast1 with Updated = DateTime.UtcNow.AddMinutes(-21.0) }, 20.0) with
        | Valid -> Assert.False(true)
        | Expired -> Assert.True(true)
        
    [<Fact>]
    let ``Validate Forecast. Given: Forecast Date = Now + 5 min; ExpirationTime = 20 min. ExpectedResult: Valid`` () =
        match Forecast.validate ({ testForecast1 with Updated = DateTime.UtcNow.AddMinutes(5.0) }, 20.0) with
        | Valid -> Assert.True(true)
        | Expired -> Assert.False(true)
