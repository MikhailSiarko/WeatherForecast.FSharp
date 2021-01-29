namespace WeatherForecast.FSharp.Storage

open WeatherForecast.FSharp.Domain
open System.Linq

type internal UserEntity = AppDbContext.dataContext.``main.UsersEntity``

module UserStorage =
    let private userLoginPredicate (login: string) (entity: UserEntity) = entity.Login.ToLower() = login.ToLower()
    
    let private saveExistingUserAsync (existing: ExistingEntity<User>) = async {
        let user = existing.Value
        let! entity = Database.singleAsync
                        <@ fun c -> c.Users :> IQueryable<_> @>
                            <@ userLoginPredicate user.Login @>
                            
        entity.Login <- user.Login
        entity.Password <- user.Password
        do! Database.saveUpdatesAsync()
        return { user with Login = entity.Login; Password = entity.Password }
    }
    
    let private saveNewUserAsync (newUser: NewEntity<User>) = async {
        let user = newUser.Value
        let entity = Database.table (fun c -> c.Users)
                     |> Database.add (fun u -> u.Login <- user.Login
                                               u.Password <- user.Password)
        do! Database.saveUpdatesAsync()
        return { user with Id = entity.Id }
    }
    
    let tryGetAsync (login: string) = async {
        let! option = Database.trySingleAsync
                        <@ fun c -> c.Users :> IQueryable<_> @>
                            <@ fun u -> u.Login.ToLower() = login.ToLower() @>
                            
        return match option with
               | Some u -> Some { Id = u.Id; Password = u.Password; Login = u.Login }
               | None -> None
    }
    
    let saveAsync (user: User) = async {
        return! match Database.exists (fun c -> c.Users :> IQueryable<_>) (fun u -> userLoginPredicate u.Login) user with
                | Exists u -> saveExistingUserAsync u
                | New u -> saveNewUserAsync u
    }