open CommandLine
open Microsoft.Extensions.DependencyInjection
open FluentMigrator.Runner
open System
open System.Reflection
open System.Text

[<Literal>]
let Up = "Up"

[<Literal>]
let Down = "Down"

type Options = {
    [<Option('t', "type", Required = true)>]
    Type: string;

    [<Option('v', "version", Required = false)>]
    Version: int64
}

let configureServices () =
    ServiceCollection().AddFluentMigratorCore().ConfigureRunner(
            fun builder ->
                builder
                    .AddSQLite()
                    .WithGlobalConnectionString("Data Source=..\\Database.db")
                    .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations
                |> ignore
        )

let migrateUp (version: int64) (runner: IMigrationRunner) =
    match version with
    | x when x = Unchecked.defaultof<int64> -> if runner.HasMigrationsToApplyUp() then runner.MigrateUp()
    | v -> if runner.HasMigrationsToApplyUp(Nullable v) then runner.MigrateUp(v)

let migrateDown (version: int64) (runner: IMigrationRunner) =
    match version with
    | x when x = Unchecked.defaultof<int64> -> failwith "Please enter the version of the migration"
    | v -> if runner.HasMigrationsToApplyDown(v) then runner.MigrateDown(v)

let run (action: IMigrationRunner -> unit) =
    let services = configureServices ()
    use scope = services.BuildServiceProvider(false).CreateScope()
    let runner = scope.ServiceProvider.GetService<IMigrationRunner>()
    action runner
    0

let resolve (result: Options) =
    match result.Type with
    | Up -> run (migrateUp result.Version)
    | Down -> run (migrateDown result.Version)
    | _ -> failwith "Please enter correct migration type"

let fold (acc: StringBuilder) (error: Error) =
    acc.AppendLine(error.ToString())

[<EntryPoint>]
let main argv =
    let result = Parser.Default.ParseArguments<Options>(argv)
    match result with
    | :? Parsed<Options> as parsed -> resolve parsed.Value
    | :? NotParsed<Options> as notParsed -> failwith ((Seq.fold fold (StringBuilder()) notParsed.Errors).ToString())
    | _ -> failwith "Error"