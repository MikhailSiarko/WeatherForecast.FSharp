namespace WeatherForecast.FSharp.Authentication

open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt
open WeatherForecast.FSharp.Domain
open System.Security.Claims
open System

type AuthenticationSource = { User: User; Token: string }

module Authentication =
    let private generateClaims (user: User) =
        [ Claim(ClaimsIdentity.DefaultNameClaimType, user.Login); Claim("Id", user.Id.ToString()) ]

    let private generateToken claims =
        let now = DateTime.Now
        JwtSecurityToken(JwtOptions.Issuer,
                         JwtOptions.Audience,
                         notBefore = Nullable now,
                         claims = claims,
                         expires = Nullable (now.Add(TimeSpan.FromMinutes(float JwtOptions.Lifetime))),
                         signingCredentials = SigningCredentials(JwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256))

    let private encodeSecurityToken = generateClaims >> generateToken >> JwtSecurityTokenHandler().WriteToken

    let getAuthenticationSource user = { User = user; Token = encodeSecurityToken user }