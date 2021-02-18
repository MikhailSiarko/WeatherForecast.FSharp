namespace WeatherForecast.FSharp.Storage

open WeatherForecast.FSharp.Domain
open System.Linq
open Database

type internal UserEntity = AppDbContext.dataContext.``main.UsersEntity``

module UserStorage =
    let private userLoginPredicate (login: string) (entity: UserEntity) =
        entity.Login.ToLower() = login.ToLower()

    let private saveExistingUserAsync (user: User) =
        async {
            let! entity = singleAsync <@ fun c -> c.Users :> IQueryable<_> @> <@ userLoginPredicate user.Login @>

            entity.Login <- user.Login
            entity.Password <- user.Password
            do! saveUpdatesAsync ()

            return
                { user with
                      Login = entity.Login
                      Password = entity.Password }
        }

    let private saveNewUserAsync (user: User) =
        async {
            let entity =
                table (fun c -> c.Users)
                |> add
                    (fun u ->
                        u.Login <- user.Login
                        u.Password <- user.Password)

            do! saveUpdatesAsync ()
            return { user with Id = entity.Id }
        }

    let tryGetAsync (login: string) =
        async {
            let! option =
                trySingleAsync
                    <@ fun c -> c.Users :> IQueryable<_> @>
                    <@ fun u -> u.Login.ToLower() = login.ToLower() @>

            return
                match option with
                | Some u ->
                    Some
                        { Id = u.Id
                          Password = u.Password
                          Login = u.Login }
                | None -> None
        }

    let saveAsync (user: User) =
        async {
            match (fun (c: MainSchema) -> c.Users :> IQueryable<_>), (fun u -> userLoginPredicate u.Login), user with
            | Exists u -> return! saveExistingUserAsync u
            | New u -> return! saveNewUserAsync u
        }
