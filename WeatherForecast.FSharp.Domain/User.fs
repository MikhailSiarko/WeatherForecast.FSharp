namespace WeatherForecast.FSharp.Domain

type Credentials = { Login: string; Password: string }

type User = { Id: int64; Credentials: Credentials }

module User =
    let (|Valid|Invalid|) (credentials: Credentials, password: string) =
        match credentials.Password = password with
        | true when isNull password -> Invalid
        | true -> Valid
        | false -> Invalid