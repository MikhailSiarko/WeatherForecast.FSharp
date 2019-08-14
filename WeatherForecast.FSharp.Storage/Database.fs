namespace WeatherForecast.FSharp.Storage

open System.Threading.Tasks
open FSharp.Data.Sql
open FSharp.Data.Sql.Common
open System.Linq

type AppDbContext = SqlDataProvider<
                        Common.DatabaseProviderTypes.SQLITE,
                        SQLiteLibrary = SQLiteLibrary.SystemDataSQLite,
                        ConnectionString = Literals.ConnectionString,
                        ResolutionPath = Literals.ResPath>

type MainSchema = AppDbContext.dataContext.mainSchema

type ExistingEntity<'a> = { Value: 'a }

type NewEntity<'a> = { Value: 'a }

type EntityStorageStatus<'e> = Exists of ExistingEntity<'e> | New of NewEntity<'e>

module Database =
    let private context = AppDbContext.GetDataContext(SelectOperations.DatabaseSide)

    let table selector = selector context.Main
    
    let query (selector: Quotations.Expr<MainSchema -> IQueryable<_>>) predicate = query {
        for u in (%selector) context.Main do
            where ((%predicate) u)
            select (u)
    }
    
    let private executeAsync selector predicate = async {
        return! predicate
                |> query selector
                |> Seq.executeQueryAsync
    }
    
    let singleAsync selector predicate = async {
        let! items = predicate
                     |> executeAsync selector
        return items
               |> Seq.head
    }
    
    let trySingleAsync selector predicate = async {
        let! items = predicate
                     |> executeAsync selector
        return items
               |> Seq.tryHead
    }
    
    let manyAsync selector predicate = async {
        return! predicate
                |> executeAsync selector
    }
    
    let deleteAsync predicateQuery = async {
        let! _ = Seq.``delete all items from single table`` predicateQuery
        return ()
    }
    
    let inline add (action: 'a -> unit) (table: ^t) =
        let entity = (^t : (member Create : unit -> 'a) table)
        action entity
        entity

    let clearUpdates () = context.ClearUpdates() |> ignore
    
    let saveUpdatesAsync obj = async {
        do! Async.AwaitTask (Task.Run(fun () -> lock context context.SubmitUpdates))
        return obj
    }
    
    let exists selector predicate obj =
        match table selector |> Seq.exists (predicate obj) with
        | true -> Exists({ Value = obj })
        | false -> New({ Value = obj })