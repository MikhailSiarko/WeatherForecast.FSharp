module ForecastTests
    open System
    open WeatherForecast.FSharp.Domain
    open Xunit

    [<Fact>]
    let ``Validate Forecast. Given: Forecast Date = Now - 21 min; ExpirationTime = 20 min. ExpectedResult: Expired`` () =
        let lastUpdateDate = DateTime.UtcNow.AddMinutes(-21.0)
        let expirationDate = DateTime.UtcNow.AddMinutes(-1.0 * float 20.0<min>)
        match Forecast.validate lastUpdateDate expirationDate with
        | Valid -> Assert.False(true)
        | Expired -> Assert.True(true)
        
    [<Fact>]
    let ``Validate Forecast. Given: Forecast Date = Now + 5 min; ExpirationTime = 20 min. ExpectedResult: Valid`` () =
        let lastUpdateDate = DateTime.UtcNow.AddMinutes(5.0)
        let expirationDate = DateTime.UtcNow.AddMinutes(-1.0 * float 20.0<min>)
        match Forecast.validate lastUpdateDate expirationDate with
        | Valid -> Assert.True(true)
        | Expired -> Assert.False(true)
