namespace WeatherForecast.FSharp.Migrate.Migrations

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
            .WithColumn("ForecastId").AsInt32().Indexed("IX_ForecastItems_ForecastId").ForeignKey("FK_ForecastItems_Forecasts_Id", "Forecasts", "Id").NotNullable()
            .WithColumn("Data").AsString().NotNullable()
            |> ignore
        ()

    override this.Down() =
        this.Delete.Table("ForecastItems") |> ignore
        this.Delete.Table("Forecasts") |> ignore
        this.Delete.Table("Users") |> ignore
        ()
