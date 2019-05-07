namespace WeatherForecast.FSharp.API.MapperProfiles

open AutoMapper
open WeatherForecast.FSharp.API.Types

type UserProfile () as self =
    inherit Profile()
    
    do self.CreateMap<UserEntity, User>(MemberList.None)
        |> mapMember <@ fun u -> u.Login @> <@ fun e -> e.Login @>
        |> mapMember <@ fun u -> u.Id @> <@ fun e -> e.Id @>
        |> ignore
