namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open FSharp.Data.Sql
open WeatherForecast.FSharp.API.Types

type ForecastProgile () as self =
    inherit Profile()
    
    do self.CreateMap<ForecastEntity, Forecast>(MemberList.None)
        |> mapMember <@ fun f -> f.Updated @> <@ fun e -> e.Created @>
        |> mapMember <@ fun f -> f.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun f -> f.CountryCode @> <@ fun e -> e.CountryCode @>
        |> mapMember <@ fun f -> f.City @> <@ fun e -> e.Location @>
        |> mapMember <@ fun f -> f.Items @> <@ fun e -> e.``main.ForecastItems by Id``
                                                                |> Seq.executeQueryAsync
                                                                |> Async.RunSynchronously
                                                                |> Seq.map map<ForecastItem>
                                                                |> Seq.toArray @>
        |> ignore