namespace WeatherForecast.FSharp.Storage

open FSharp.Data.Sql
open FSharp.Data.Sql.Common

module Literals =
    [<Literal>]
    let ConnectionString = "Data Source=" + __SOURCE_DIRECTORY__ + "/../Database.db"

    [<Literal>]
    let ResPath = __SOURCE_DIRECTORY__ + "../packages/system.data.sqlite.core/1.0.111/lib/netstandard2.0"

type AppDbContext = SqlDataProvider<
                        Common.DatabaseProviderTypes.SQLITE,
                        SQLiteLibrary = SQLiteLibrary.SystemDataSQLite,
                        ConnectionString = Literals.ConnectionString,
                        ResolutionPath = Literals.ResPath>