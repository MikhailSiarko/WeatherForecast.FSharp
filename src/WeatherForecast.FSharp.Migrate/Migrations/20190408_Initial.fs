namespace WeatherForecast.FSharp.Migrate.Migrations

open System.Data
open FluentMigrator

[<Migration(20190408111600L)>]
type Initial () =
    inherit Migration()

    override this.Up() =
        this.Create.Table("Users")
            .WithColumn("Id").AsInt32().Indexed("IX_Users_Id").Identity().PrimaryKey("PK_Users").NotNullable()
            .WithColumn("Login").AsFixedLengthString(30).Unique().NotNullable()
            .WithColumn("Password").AsString().NotNullable()
            |> ignore

        this.Create
            .Table("Forecasts")
            .WithColumn("Id").AsInt32().Indexed("IX_Forecasts_Id").Identity().PrimaryKey("PK_Forecasts").NotNullable()
            .WithColumn("CountryCode").AsFixedLengthString(10).NotNullable()
            .WithColumn("Location").AsFixedLengthString(100).NotNullable()
            .WithColumn("Created").AsDateTime().NotNullable()
        |> ignore

        this.Create.Table("ForecastItems")
            .WithColumn("Id").AsInt32().Indexed("IX_ForecastItems_Id").Identity().PrimaryKey("PK_ForecastItems").NotNullable()
            .WithColumn("ForecastId").AsInt32().Indexed("IX_ForecastItems_ForecastId").ForeignKey("FK_ForecastItems_Forecasts_Id", "Forecasts", "Id").OnDelete(Rule.Cascade).NotNullable()
            .WithColumn("Date").AsDateTime().NotNullable()
            |> ignore

        this.Create.Table("ForecastTimeItems")
            .WithColumn("Id").AsInt32().Indexed("IX_ForecastTimeItems_Id").Identity().PrimaryKey("PK_ForecastTimeItems").NotNullable()
            .WithColumn("Time").AsTime().NotNullable()
            .WithColumn("ForecastItemId").AsInt32().Indexed("IX_ForecastTimeItems_ForecastItemId").ForeignKey("FK_ForecastTimeItems_ForecastItems_Id", "ForecastItems", "Id").OnDelete(Rule.Cascade).NotNullable()
        |> ignore

        this.Create.Table("Mains")
            .WithColumn("Id").AsInt32().Indexed("IX_Mains_Id").Identity().PrimaryKey("PK_Mains").NotNullable()
            .WithColumn("Temp").AsDecimal().NotNullable()
            .WithColumn("MinTemp").AsDecimal().NotNullable()
            .WithColumn("MaxTemp").AsDecimal().NotNullable()
            .WithColumn("Pressure").AsDecimal().NotNullable()
            .WithColumn("Humidity").AsInt32().NotNullable()
            .WithColumn("ForecastTimeItemId").AsInt32().Indexed("IX_Mains_ForecastTimeItemId").ForeignKey("FK_Mains_ForecastTimeItems_Id", "ForecastTimeItems", "Id").OnDelete(Rule.Cascade).NotNullable()
        |> ignore

        this.Create
            .Table("Weathers")
            .WithColumn("Id").AsInt32().Indexed("IX_Weathers_Id").Identity().PrimaryKey("PK_Weathers").NotNullable()
            .WithColumn("Main").AsFixedLengthString(50).NotNullable()
            .WithColumn("Description").AsFixedLengthString(100).NotNullable()
            .WithColumn("Icon").AsString().Nullable()
            .WithColumn("ForecastTimeItemId").AsInt32().Indexed("IX_Weathers_ForecastTimeItemId").ForeignKey("FK_Weathers_ForecastTimeItems_Id", "ForecastTimeItems", "Id").OnDelete(Rule.Cascade).NotNullable()
        |> ignore

        this.Create.Table("Winds")
            .WithColumn("Id").AsInt32().Indexed("IX_Winds_Id").Identity().PrimaryKey("PK_Winds").NotNullable()
            .WithColumn("Speed").AsDecimal().NotNullable()
            .WithColumn("Degree").AsDecimal().NotNullable()
            .WithColumn("ForecastTimeItemId").AsInt32().Indexed("IX_Winds_ForecastTimeItemId").ForeignKey("FK_Winds_ForecastTimeItems_Id", "ForecastTimeItems", "Id").OnDelete(Rule.Cascade).NotNullable()
        |> ignore
        ()

    override this.Down() =
        this.Delete.Table("ForecastItems") |> ignore
        this.Delete.Table("Forecasts") |> ignore
        this.Delete.Table("ForecastTimeItems") |> ignore
        this.Delete.Table("Users") |> ignore
        this.Delete.Table("Mains") |> ignore
        this.Delete.Table("Weathers") |> ignore
        this.Delete.Table("Winds") |> ignore
        ()
