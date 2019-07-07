namespace WeatherForecast.FSharp.Migrate.Migrations

open FluentMigrator

[<Migration(20190707111600L)>]
type AddIcon () =
    inherit Migration()

    override this.Up() =
        this.Alter.Table("Weathers").AddColumn("Icon").AsString().Nullable() |> ignore
        ()

    override this.Down() =
        this.Delete.Column("Icon").FromTable("Weathers") |> ignore
        ()
