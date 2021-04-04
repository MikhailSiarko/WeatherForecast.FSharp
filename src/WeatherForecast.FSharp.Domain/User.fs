namespace WeatherForecast.FSharp.Domain

type Credentials = { Login: string; Password: string }

type User =
    { Id: int64
      Login: string
      Password: string }

type PasswordStatus =
    | Valid
    | Invalid

module User =
    let validatePassword enteredPassword userPassword =
        match enteredPassword = userPassword with
        | true when isNull enteredPassword -> Invalid
        | true -> Valid
        | false -> Invalid
