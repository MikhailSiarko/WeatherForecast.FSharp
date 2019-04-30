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

type ForecastEntity = AppDbContext.dataContext.``main.ForecastsEntity``
type ForecastItemEntity = AppDbContext.dataContext.``main.ForecastItemsEntity``
type MainEntity = AppDbContext.dataContext.``main.MainsEntity``
type WeatherEntity = AppDbContext.dataContext.``main.WeathersEntity``
type WindEntity = AppDbContext.dataContext.``main.WindsEntity``
type UserEntity = AppDbContext.dataContext.``main.UsersEntity``