namespace WeatherForecast.FSharp.Storage

module Literals =
    [<Literal>]
    let ConnectionString = "Data Source=" + __SOURCE_DIRECTORY__ + "/../Database.db;foreign keys=true"

    [<Literal>]
    let ResPath = __SOURCE_DIRECTORY__ + "../packages/system.data.sqlite.core/1.0.111/lib/netstandard2.0"
