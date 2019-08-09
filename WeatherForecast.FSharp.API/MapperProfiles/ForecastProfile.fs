namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type ForecastProfile () as self =
    inherit Profile()
    
    do self.CreateMap<ForecastEntity, Forecast>(MemberList.None)
        |> mapMember <@ fun f -> f.Updated @> <@ fun e -> e.Created @>
        |> mapMember <@ fun f -> f.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun f -> f.CountryCode @> <@ fun e -> e.CountryCode @>
        |> mapMember <@ fun f -> f.City @> <@ fun e -> e.Location @>
        |> mapMember <@ fun f -> f.Items @> <@ fun _ -> [] @>
        |> ignore