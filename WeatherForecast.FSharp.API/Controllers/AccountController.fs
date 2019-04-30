namespace WeatherForecast.FSharp.API.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open WeatherForecast.FSharp.API.Types
open WeatherForecast.FSharp.API.Modules

[<Route("api/[controller]")>]
[<ApiController>]
[<AllowAnonymous>]
type AccountController () =
    inherit ControllerBase()

    [<HttpPost("login")>]
    member __.Login([<FromBody>] loginData: LoginData) = async {
        let! user = Account.loginAsync loginData
        return JsonResult(user)
    }

    [<HttpPost("register")>]
    member __.Register([<FromBody>] registerData: RegisterData) = async {
        let! user = Account.registerAsync registerData
        return JsonResult(user)
    }