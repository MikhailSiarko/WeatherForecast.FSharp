namespace WeatherForecast.FSharp.API.Modules

module Account =
    open WeatherForecast.FSharp.API.Modules
    open WeatherForecast.FSharp.API.Types

    let private validatePasswords first second =
        if first = second then
            ()
        else
            failwith "Passwords do not match"

    let loginAsync (loginData: LoginData) = async {
        let! userOption = Database.tryGetUserAsync loginData.Login
        return match userOption with
                | Some x when x.Password = (Encryption.encrypt loginData.Password) -> Authentication.authenticate x
                | Some _ -> failwith "You've entered an incorrect password"
                | None -> failwithf "User %s wasn't found" loginData.Login
    }

    let registerAsync (registerData: RegisterData) = async {
        let! userOption = Database.tryGetUserAsync registerData.Login
        return match userOption with
                | None -> validatePasswords registerData.Password registerData.ConfirmPassword |> ignore
                          Database.clearUpdates ()
                          Encryption.encrypt registerData.Password
                          |> Database.createUserAsync registerData.Login
                          |> Async.RunSynchronously
                          |> Authentication.authenticate
                | Some x -> failwithf "User with login %s already exists" x.Login
    }