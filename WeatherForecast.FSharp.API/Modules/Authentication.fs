namespace WeatherForecast.FSharp.API.Modules

module Authentication =
    open Microsoft.IdentityModel.Tokens
    open WeatherForecast.FSharp.API.Types.Application
    open WeatherForecast.FSharp.API.Types.Authentication
    open System.IdentityModel.Tokens.Jwt
    open System.Security.Claims
    open System

    let private generateClaims user =
        [ Claim(ClaimsIdentity.DefaultNameClaimType, user.Login); Claim("Id", user.Id.ToString()) ]

    let private generateToken claims =
        let now = DateTime.Now
        JwtSecurityToken(
                            JwtOptions.Issuer,
                            JwtOptions.Audience,
                            notBefore = Nullable now,
                            claims = claims,
                            expires = Nullable (now.Add(TimeSpan.FromMinutes(float JwtOptions.Lifetime))),
                            signingCredentials = SigningCredentials(JwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256)
                        )

    let private encodeSecurityToken = generateClaims >> generateToken >> JwtSecurityTokenHandler().WriteToken

    let authenticate user =
        user
        |> encodeSecurityToken
        |> AuthenticationData.Create user