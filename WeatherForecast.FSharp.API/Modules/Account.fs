namespace WeatherForecast.FSharp.API.Modules
open System.Linq

module Account =
    open WeatherForecast.FSharp.API.Modules
    open WeatherForecast.FSharp.API.Types

    let private validatePasswords first second =
        if first = second then
            ()
        else
            failwith "Passwords do not match"
    
    let private getUserAsync login = async {
        return! Database.tableQuery <@ fun m -> m.Users :> IQueryable<_> @>
                |> Database.where <@ fun u -> u.Login = login @>
                |> Database.singleOrDefaultAsync
    }
    
    let loginAsync (loginData: LoginData) = async {
        let! userOption = getUserAsync loginData.Login
        return match userOption with
                | Some x when x.Password = (Encryption.encrypt loginData.Password) -> Authentication.authenticate x
                | Some _ -> failwith "You've entered an incorrect password"
                | None -> failwithf "User %s wasn't found" loginData.Login
    }

    let registerAsync (registerData: RegisterData) = async {
        let! userOption = getUserAsync registerData.Login
        return match userOption with
                | None -> validatePasswords registerData.Password registerData.ConfirmPassword |> ignore
                          Database.clearUpdates ()
                          let encrypted = Encryption.encrypt registerData.Password
                          Database.table (fun m -> m.Users)
                          |> Database.add (fun u -> u.Login <- registerData.Login; u.Password <- encrypted)
                          |> Database.saveUpdatesAsync
                          |> Async.RunSynchronously
                          |> Authentication.authenticate
                | Some x -> failwithf "User with login %s already exists" x.Login
    }