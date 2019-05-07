namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open FSharp.Data.Sql
open WeatherForecast.FSharp.API.Types

type ForecastItemProfile () as this =
    inherit Profile()
    
    do this.CreateMap<ForecastItemEntity, ForecastItem>(MemberList.None)
        |> mapMember <@ fun i -> i.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun i -> i.Date @> <@ fun e -> e.Date @>
        |> mapMember <@ fun i -> i.ForecastId @> <@ fun e -> e.ForecastId @>
        |> mapMember <@ fun i -> i.Main @> <@ fun e -> e.``main.Mains by Id``
                                                                        |> Seq.headAsync
                                                                        |> Async.RunSynchronously
                                                                        |> map<Main> @>
        |> mapMember <@ fun i -> i.Weather @> <@ fun e -> e.``main.Weathers by Id``
                                                                    |> Seq.executeQueryAsync
                                                                    |> Async.RunSynchronously
                                                                    |> Seq.map map<Weather>
                                                                    |> Seq.toArray @>
        |> mapMember <@ fun i -> i.Wind @> <@ fun e -> e.``main.Winds by Id``
                                                                |> Seq.headAsync
                                                                |> Async.RunSynchronously
                                                                |> map<Wind> @>
        |> ignore
