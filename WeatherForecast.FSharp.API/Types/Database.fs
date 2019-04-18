module WeatherForecast.FSharp.API.Types.Database

open FSharp.Data.Sql
open FSharp.Data.Sql.Common

[<Literal>]
let ConnectionString = "Data Source=" + __SOURCE_DIRECTORY__ + "\\..\\..\\Database.db"

[<Literal>]
let ResPath = "%USERPROFILE%\\.nuget\\packages\\system.data.sqlite.core\\1.0.110\\lib\\netstandard2.0"

type AppDbContext = SqlDataProvider<
                        Common.DatabaseProviderTypes.SQLITE,
                        SQLiteLibrary = SQLiteLibrary.SystemDataSQLite,
                        ConnectionString = ConnectionString,
                        ResolutionPath = ResPath>