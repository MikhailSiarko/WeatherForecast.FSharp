#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Testing.XUnit2
nuget Fake.Core.Target
nuget Fake.DotNet.Paket //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.Testing

let solution =
    __SOURCE_DIRECTORY__
    @@ "/WeatherForecast.FSharp.sln"

let srcDir = __SOURCE_DIRECTORY__ @@ "/src"

let testDir = __SOURCE_DIRECTORY__ @@ "/tests"

let sqlInteropFileName = "SQLite.Interop.dll"

let getPlatformFolder () =
    if Environment.isMacOS then
        "osx-x64"
    elif Environment.isLinux then
        "linux-x64"
    else
        "win-x64"

let sqlLibPath =
    __SOURCE_DIRECTORY__
    </> "packages"
    </> "Stub.System.Data.SQLite.Core.NetStandard"
    </> "lib"
    </> "netstandard2.1"
    </> sqlInteropFileName

let sqlInteropPath =
    __SOURCE_DIRECTORY__
    </> "packages"
    </> "Stub.System.Data.SQLite.Core.NetStandard"
    </> "runtimes"
    </> getPlatformFolder ()
    </> "native"
    </> sqlInteropFileName

Target.create
    "Clean"
    (fun _ ->
        !!(srcDir @@ "/**/bin")
        ++ (testDir @@ "/**/bin")
        |> Shell.cleanDirs)

Target.create
    "CopySqlInterop"
    (fun _ ->
        match (File.exists sqlInteropPath)
              && (not (File.exists sqlLibPath)) with
        | true -> Shell.copyFile sqlLibPath sqlInteropPath
        | false -> Trace.logToConsole ($"The %s{sqlInteropFileName} was already copied", Trace.Information))

Target.create
    "Restore"
    (fun _ ->
        Paket.restore
            (fun o ->
                { o with
                      ToolType = ToolType.CreateCLIToolReference() }))

Target.create
    "Build"
    (fun _ ->
        DotNet.build
            (fun o ->
                { o with
                      NoRestore = true
                      Configuration = DotNet.Debug })
            solution)

Target.create
    "Test"
    (fun _ ->
        !!(testDir @@ "/**/bin/**/net*/*.Tests.dll")
        |> XUnit2.run (fun o -> { o with Parallel = XUnit2.All }))

open Fake.Core.TargetOperators


"Restore"
==> "CopySqlInterop"
==> "Clean"
==> "Build"
==> "Test"

Target.runOrDefault "Build"
