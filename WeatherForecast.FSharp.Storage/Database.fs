namespace WeatherForecast.FSharp.Storage
open System.Threading.Tasks

type MainSchema = AppDbContext.dataContext.mainSchema

type ExistingEntity<'a> = { Value: 'a }

type NewEntity<'a> = { Value: 'a }

type EntityStorageStatus<'e> = Exists of ExistingEntity<'e> | New of NewEntity<'e>

module Database =
    open FSharp.Data.Sql

    let private context = AppDbContext.GetDataContext(SelectOperations.DatabaseSide)

    let table selector = selector context.Main
    
    let query selector predicate map = query {
        for u in (%selector) context.Main do
            where ((%predicate) u)
            select ((%map) u)
    }
    
    let singleAsync selector predicate map = async {
        return! map
                |> query selector predicate
                |> Seq.headAsync
    }
    
    let trySingleAsync selector predicate map = async {
        return! map
                |> query selector predicate
                |> Seq.tryHeadAsync
    }
    
    let manyAsync selector predicate map = async {
        return! map
                |> query selector predicate
                |> Seq.executeQueryAsync
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