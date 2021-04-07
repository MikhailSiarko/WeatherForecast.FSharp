namespace WeatherForecast.FSharp.Storage

module Literals =
    [<Literal>]
    let ConnectionString =
        "Data Source="
        + __SOURCE_DIRECTORY__
        + "/../../Database.db;foreign keys=true"

    [<Literal>]
    let ResPath =
        __SOURCE_DIRECTORY__
        + "/../../packages/Stub.System.Data.SQLite.Core.NetStandard/lib/netstandard2.1"
