namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type ForecastTimeItemProfile () as this =
    inherit Profile()
    
    do this.CreateMap<ForecastTimeItemEntity, ForecastTimeItem>(MemberList.None)
        |> mapMember <@ fun i -> i.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun i -> i.Time @> <@ fun e -> e.Time @>
        |> mapMember <@ fun i -> i.ForecastItemId @> <@ fun e -> e.ForecastItemId @>
        |> mapMember <@ fun i -> i.Main @> <@ fun _ -> null @>
        |> mapMember <@ fun i -> i.Weather @> <@ fun _ -> null @>
        |> mapMember <@ fun i -> i.Wind @> <@ fun _ -> null @>
        |> ignore
