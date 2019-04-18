namespace WeatherForecast.FSharp.API.Migrations

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

        this.Create.Table("ForecastItems")
            .WithColumn("Id").AsInt32().Indexed("IX_ForecastItems_Id").Identity().PrimaryKey("PK_ForecastItems").NotNullable()
            .WithColumn("UserId").AsInt32().Indexed("IX_ForecastItems_UserId").ForeignKey("FK_ForecastItems_Users_Id", "Users", "Id")
            |> ignore
        ()

    override this.Down() =
        this.Delete.Table("ForecastItems") |> ignore
        this.Delete.Table("Users") |> ignore
        ()
