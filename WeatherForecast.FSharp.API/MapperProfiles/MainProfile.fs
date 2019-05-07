namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type MainProfile () as this =
    inherit Profile()
    
    do this.CreateMap<MainEntity, Main>(MemberList.None)
        |> mapMember <@ fun m -> m.Id @> <@ fun e -> e.Id @>
        |> mapMember <@ fun m -> m.Temp @> <@ fun e -> e.Temp @>
        |> mapMember <@ fun m -> m.MaxTemp @> <@ fun e -> e.MaxTemp @>
        |> mapMember <@ fun m -> m.MinTemp @> <@ fun e -> e.MinTemp @>
        |> mapMember <@ fun m -> m.Pressure @> <@ fun e -> e.Pressure @>
        |> mapMember <@ fun m -> m.Humidity @> <@ fun e -> e.Humidity @>
        |> ignore