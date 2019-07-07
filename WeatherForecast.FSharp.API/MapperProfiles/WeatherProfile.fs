namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type WeatherProfile () as this =
    inherit Profile()
    
    do this.CreateMap<WeatherEntity, Weather>(MemberList.None)
        |> mapMember <@ fun w -> w.Id @> <@ fun i -> i.Id @>
        |> mapMember <@ fun w -> w.Main @> <@ fun i -> i.Main @>
        |> mapMember <@ fun w -> w.Description @> <@ fun i -> i.Description @>
        |> mapMember <@ fun w -> w.Icon @> <@ fun i -> i.Icon @>
        |> ignore