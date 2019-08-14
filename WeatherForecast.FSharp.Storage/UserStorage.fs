namespace WeatherForecast.FSharp.Storage

open WeatherForecast.FSharp.Domain

type UserEntity = AppDbContext.dataContext.``main.UsersEntity``

module UserStorage =
    let private userLoginPredicate (login: string) (entity: UserEntity) = entity.Login.ToLower() = login.ToLower()
    
    let private saveExistingUserAsync (existing: ExistingEntity<User>) = async {
        let user = existing.Value
        let! entity = Database.singleAsync
                        <@ fun c -> c.Users @>
                            <@ userLoginPredicate user.Credentials.Login @>
                                <@ fun u -> u @>
        entity.Login <- user.Credentials.Login
        entity.Password <- user.Credentials.Password
        do! Database.saveUpdatesAsync()
        return { user with Credentials = { Login = entity.Login; Password = entity.Password } }
    }
    
    let private saveNewUserAsync (newUser: NewEntity<User>) = async {
        let user = newUser.Value
        let entity = Database.table (fun c -> c.Users)
                     |> Database.add (fun u -> u.Login <- user.Credentials.Login
                                               u.Password <- user.Credentials.Password)
        do! Database.saveUpdatesAsync()
        return { user with Id = entity.Id }
    }
    
    let tryGetAsync login = async {
        return! Database.trySingleAsync
                    <@ fun c -> c.Users @>
                        <@ userLoginPredicate login @>
                            <@ fun u -> { Id = u.Id; Credentials = { Password = u.Password; Login = u.Login } } @>
    }
    
    let saveAsync (user: User) = async {
        return! match Database.exists (fun c -> c.Users) (fun u -> userLoginPredicate u.Credentials.Login) user with
                | Exists u -> saveExistingUserAsync u
                | New u -> saveNewUserAsync u
    }