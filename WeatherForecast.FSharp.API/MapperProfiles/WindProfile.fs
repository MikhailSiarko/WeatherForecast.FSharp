namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type WindProfile () as this =
    inherit Profile()
    
    do this.CreateMap<WindEntity, Wind>(MemberList.None)
        |> mapMember <@ fun w -> w.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun w -> w.Degree @> <@ fun e -> e.Degree @>
        |> mapMember <@ fun w -> w.Speed @> <@ fun e -> e.Speed @>
        |> ignore
