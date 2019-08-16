module UserTests
    open Xunit
    open WeatherForecast.FSharp.Domain
    
    [<Fact>]
    let ``Validate Credentials. Given: Login: 'm$mail.com', Password: '123456', Password to compare: '123456'; Expected Result": Valid`` () =
        let password = "123456"
        let user = { Id = 1L; Login = "login"; Password = "123456" }
        
        match User.validatePassword password user with
        | Valid u ->
            Assert.IsType(typeof<User>, u)
            Assert.Equal(user, u)
            Assert.True(true)
        | Invalid -> Assert.False(true)
        
    [<Fact>]
    let ``Validate Credentials. Given: Login: 'm$mail.com', Password: '123456', Password to compare: '654321'; Expected Result": Invalid`` () =
        let password = "123456"
        let user = { Id = 1L; Login = "login"; Password = "654321" }
        
        match User.validatePassword password user with
        | Valid _ -> Assert.False(true)
        | Invalid -> Assert.True(true)
        
    [<Fact>]
    let ``Validate Credentials. Given: Login: 'm$mail.com', Password: '123456', Password to compare: null; Expected Result": Invalid`` () =
        let password = null
        let user = { Id = 1L; Login = "login"; Password = "654321" }
        
        match User.validatePassword password user with
        | Valid _ -> Assert.False(true)
        | Invalid -> Assert.True(true)
