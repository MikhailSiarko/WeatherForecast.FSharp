namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open WeatherForecast.FSharp.API.AccountData
open WeatherForecast.FSharp.API.Modules
open Microsoft.AspNetCore.Mvc

[<Route("api/[controller]")>]
[<ApiController>]
[<AllowAnonymous>]
type AccountController () =
    inherit ControllerBase()

    [<HttpPost("login")>]
    member this.Login([<FromBody>] loginData: LoginData) = async {
        let! user = Account.loginAsync { Login = loginData.Login; Password = loginData.Password }
        return this.Ok(user)
    }

    [<HttpPost("register")>]
    member this.Register([<FromBody>] registerData: RegisterData) = async {
        let! user = Account.registerAsync (registerData.Login, registerData.Password, registerData.ConfirmPassword)
        return this.Ok(user)
    }