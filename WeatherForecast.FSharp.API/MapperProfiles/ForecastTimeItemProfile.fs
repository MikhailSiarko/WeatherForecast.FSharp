namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open FSharp.Data.Sql
open WeatherForecast.FSharp.API.Types

type ForecastTimeItemProfile () as this =
    inherit Profile()
    
    do this.CreateMap<ForecastTimeItemEntity, ForecastTimeItem>(MemberList.None)
        |> mapMember <@ fun i -> i.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun i -> i.Time @> <@ fun e -> e.Time @>
        |> mapMember <@ fun i -> i.ForecastItemId @> <@ fun e -> e.ForecastItemId @>
        |> mapMember <@ fun i -> i.Main @> <@ fun e -> e.``main.Mains by Id``
                                                                |> Seq.headAsync
                                                                |> Async.RunSynchronously
                                                                |> mapTo<Main> @>
        |> mapMember <@ fun i -> i.Weather @> <@ fun e -> e.``main.Weathers by Id``
                                                                    |> Seq.executeQueryAsync
                                                                    |> Async.RunSynchronously
                                                                    |> Seq.map mapTo<Weather>
                                                                    |> Seq.toArray @>
        |> mapMember <@ fun i -> i.Wind @> <@ fun e -> e.``main.Winds by Id``
                                                                |> Seq.headAsync
                                                                |> Async.RunSynchronously
                                                                |> mapTo<Wind> @>
        |> ignore
