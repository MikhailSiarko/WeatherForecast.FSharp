namespace WeatherForecast.FSharp.API.Modules

module Database =
    open System.Linq
    open FSharp.Data.Sql
    open WeatherForecast.FSharp.API.Types

    let private context = AppDbContext.GetDataContext(SelectOperations.DatabaseSide)

    let table selector = selector context.Main
    
    let tableQuery (tableSelectorExpr: Quotations.Expr<MainSchema -> IQueryable<_>>) predicateExpr =
        query {
            for entity in ((%tableSelectorExpr) context.Main) do
                where ((%predicateExpr) entity)
        }
    
    let where predicateExpr tableQuery = tableQuery predicateExpr 
    
    let singleOrDefaultAsync contextQuery = async {
        return! Seq.tryHeadAsync contextQuery
    }
    
    let deleteAsync predicateQuery = async {
        return! Seq.``delete all items from single table`` predicateQuery
    }
    
    let inline add action (table: ^t) =
        let entity = (^t : (member Create : unit -> 'a) (table))
        action entity
        entity
    
    let update action entity =
        action entity
        entity

    let clearUpdates () = context.ClearUpdates() |> ignore
    
    let saveUpdatesAsync obj = async {
        do! context.SubmitUpdatesAsync()
        return obj
    }