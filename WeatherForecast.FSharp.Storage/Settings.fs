namespace WeatherForecast.FSharp.Storage

module Literals =
    [<Literal>]
    let ConnectionString =
        "Data Source="
        + __SOURCE_DIRECTORY__
        + "/../Database.db;foreign keys=true"

    [<Literal>]
    let ResPath =
        __SOURCE_DIRECTORY__
        + "../packages/stub.system.data.sqlite.core.netstandard/1.0.113.2/lib/netstandard2.1"
