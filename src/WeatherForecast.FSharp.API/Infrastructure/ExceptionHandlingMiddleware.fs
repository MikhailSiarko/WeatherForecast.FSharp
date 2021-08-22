namespace WeatherForecast.FSharp.API.Infrastructure

open Microsoft.AspNetCore.Http
open FSharp.Data
open Newtonsoft.Json
open System
open System.Threading.Tasks

type ExceptionHandlingMiddleware(next: RequestDelegate) =
    member this.InvokeAsync(context: HttpContext) : Task =
        async {
            try
                do! next.Invoke(context) |> Async.AwaitTask
            with
            | :? OperationCanceledException -> return! Async.AwaitTask Task.CompletedTask
            | e -> do! this.HandleExceptionAsync context e.Message
        }
        |> Async.StartAsTask
        :> Task



    member private __.HandleExceptionAsync (context: HttpContext) message =
        context.Response.ContentType <- HttpContentTypes.Json
        context.Response.StatusCode <- StatusCodes.Status500InternalServerError

        let result =
            JsonConvert.SerializeObject {| ErrorMessage = message |}

        context.Response.WriteAsync(result)
        |> Async.AwaitTask
