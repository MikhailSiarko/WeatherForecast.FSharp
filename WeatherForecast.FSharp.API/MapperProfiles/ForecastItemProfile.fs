namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type ForecastItemProfile () as this =
    inherit Profile()
    
    do this.CreateMap<ForecastItemEntity, ForecastItem>(MemberList.None)
        |> mapMember <@ fun i -> i.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun i -> i.Date @> <@ fun e -> e.Date @>
        |> mapMember <@ fun i -> i.ForecastId @> <@ fun e -> e.ForecastId @>
        |> mapMember <@ fun i -> i.TimeItems @> <@ fun _ -> [] @>
        |> ignore
