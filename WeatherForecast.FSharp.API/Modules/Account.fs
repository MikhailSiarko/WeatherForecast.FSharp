namespace WeatherForecast.FSharp.API.Modules

module Account =
    open WeatherForecast.FSharp.API.Types
    open WeatherForecast.FSharp.API.Modules
    open FSharp.Data.Sql

    let private dbContext = AppDbContext.GetDataContext()

    let private buildUserQuery (loginData: LoginData) = query {
        for u in dbContext.Main.Users do
            where (u.Login = loginData.Login && u.Password = loginData.Password)
            select { Id = u.Id; Login = u.Login }
    }

    let private validatePasswords first second =
        if first = second then
            ()
        else
            failwith "Passwords do not match"

    let loginAsync loginData = async {
        let encryptedLoginData = { loginData with LoginData.Password = Encryption.encrypt loginData.Password }

        let! userOption = encryptedLoginData
                            |> buildUserQuery
                            |> Seq.tryHeadAsync

        return match userOption with
                | None -> failwithf "User %s wasn't found" loginData.Login
                | Some user -> user |> Authentication.authenticate
    }

    let registerAsync (registerData: RegisterData) = async {
        validatePasswords registerData.Password registerData.ConfirmPassword |> ignore
        let encrypted = Encryption.encrypt registerData.Password
        dbContext.ClearUpdates() |> ignore
        let entity = dbContext.Main.Users.``Create(Login, Password)``(registerData.Login, encrypted)
        do! dbContext.SubmitUpdatesAsync()
        return Authentication.authenticate { Id = entity.Id; Login = entity.Login }
    }