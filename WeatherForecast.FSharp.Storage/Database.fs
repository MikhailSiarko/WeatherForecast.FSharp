namespace WeatherForecast.FSharp.Storage

open System.Threading.Tasks
open FSharp.Data.Sql
open FSharp.Data.Sql.Common
open System.Linq

type internal AppDbContext =
    SqlDataProvider<
        DatabaseProviderTypes.SQLITE,
        SQLiteLibrary=SQLiteLibrary.SystemDataSQLite,
        ConnectionString=Literals.ConnectionString,
        ResolutionPath=Literals.ResPath>

type internal MainSchema = AppDbContext.dataContext.mainSchema

type EntityStorageStatus<'e> =
    | Exists of 'e
    | New of 'e

module Database =
    let private context =
        AppDbContext.GetDataContext(SelectOperations.DatabaseSide)

    let table selector = selector context.Main

    let queryTo (selector: Quotations.Expr<MainSchema -> #IQueryable<_>>) predicate =
        query {
            for u in (%selector) context.Main do
                where ((%predicate) u)
                select u
        }

    let select (selector: Quotations.Expr<'a -> 'b>) (queryable: #IQueryable<'a>) =
        query {
            for t in queryable do
                select ((%selector) t)
        }

    let private executeAsync selector predicate =
        async {
            return!
                predicate
                |> queryTo selector
                |> Seq.executeQueryAsync
        }

    let singleAsync selector predicate =
        async {
            let! items = predicate |> executeAsync selector
            return items |> Seq.head
        }

    let trySingleAsync selector predicate =
        async {
            let! items = predicate |> executeAsync selector
            return items |> Seq.tryHead
        }

    let manyAsync selector predicate =
        async { return! predicate |> executeAsync selector }

    let inline add (action: 'a -> unit) (table: ^t) =
        let entity = (^t: (member Create: unit -> 'a) table)
        action entity
        entity

    let clearUpdates () = context.ClearUpdates() |> ignore

    let saveUpdatesAsync obj =
        async {
            do! Async.AwaitTask(Task.Run(fun () -> lock context context.SubmitUpdates))
            return obj
        }

    let (|Exists|New|) (selector, predicate, obj) =
        match table selector |> Seq.exists (predicate obj) with
        | true -> Exists obj
        | false -> New obj
