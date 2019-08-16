namespace WeatherForecast.FSharp.Domain

type Credentials = { Login: string; Password: string }

type User = { Id: int64; Login: string; Password: string }

type PasswordStatus = Valid of User | Invalid

module User =
    let validatePassword password user =
        match password = user.Password with
        | true when isNull password -> Invalid
        | true -> Valid(user)
        | false -> Invalid