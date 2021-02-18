namespace WeatherForecast.FSharp.API.Modules

open WeatherForecast.FSharp.Authentication
open WeatherForecast.FSharp.Domain
open WeatherForecast.FSharp.Storage

type UserInfo = { Id: int64; Login: string }

type AuthenticationData =
    { Token: string
      User: UserInfo }
    static member Create(authSource: AuthenticationSource) =
        { Token = authSource.Token
          User =
              { Id = authSource.User.Id
                Login = authSource.User.Login } }

type PasswordConfirmationResult =
    | Confirmed
    | NotConfirmed

module Account =
    let private auth =
        (Authentication.getAuthenticationSource
         >> AuthenticationData.Create)

    let private processUserResult login =
        function
        | Some u -> u
        | None -> failwithf $"User %s{login} wasn't found"

    let private processPasswordValidation onValid =
        function
        | PasswordStatus.Valid user -> onValid user
        | Invalid -> failwith "You've entered an incorrect password"

    let private passwordsConfirmed (pas, conf) =
        match pas = conf with
        | true -> Confirmed
        | false -> NotConfirmed

    let private registerUserAsync login password =
        async {
            let! userOption = UserStorage.tryGetAsync login

            return
                match userOption with
                | None ->
                    { Id = Unchecked.defaultof<int64>
                      Login = login
                      Password = Encryption.encrypt password }
                    |> UserStorage.saveAsync
                    |> Async.RunSynchronously
                    |> auth
                | Some u -> failwithf $"User with login %s{u.Login} already exists"
        }

    let loginAsync (credentials: Credentials) =
        async {
            let! userOption = UserStorage.tryGetAsync credentials.Login

            return
                userOption
                |> processUserResult credentials.Login
                |> (Encryption.encrypt >> User.validatePassword) credentials.Password
                |> processPasswordValidation auth
        }

    let registerAsync login password confirmPassword =
        async {
            return!
                match passwordsConfirmed (password, confirmPassword) with
                | Confirmed -> registerUserAsync login password
                | NotConfirmed -> failwith "Password is not confirmed"
        }
