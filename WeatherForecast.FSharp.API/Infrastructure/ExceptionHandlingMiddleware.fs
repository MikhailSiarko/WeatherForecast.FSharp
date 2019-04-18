namespace WeatherForecast.FSharp.API.Infrastructure

open Microsoft.AspNetCore.Http
open FSharp.Data
open System
open System.Threading.Tasks

type ExeptionHandlingMiddleware (next: RequestDelegate) =
    member this.InvokeAsync(context: HttpContext) : Task =
        async {
            try
                do! next.Invoke(context) |> Async.AwaitTask
            with
            | :? OperationCanceledException -> return! Task.CompletedTask |> Async.AwaitTask
            | e -> do! this.HandleExceptionAsync context e.Message
        } |> Async.StartAsTask :> Task



    member private __.HandleExceptionAsync (context: HttpContext) message =
        context.Response.ContentType <- HttpContentTypes.Json;
        context.Response.StatusCode <- StatusCodes.Status500InternalServerError
        context.Response.WriteAsync(message) |> Async.AwaitTask;