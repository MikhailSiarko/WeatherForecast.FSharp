namespace WeatherForecast.FSharp.API.AccountData

type LoginData = { Login: string; Password: string }

type RegisterData =
    { Login: string
      Password: string
      ConfirmPassword: string }
