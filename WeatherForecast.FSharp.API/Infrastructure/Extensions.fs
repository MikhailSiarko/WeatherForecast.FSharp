namespace WeatherForecast.FSharp.API.Infrastructure

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.AspNetCore.Authentication.JwtBearer
    open WeatherForecast.FSharp.Authentication
    open Microsoft.IdentityModel.Tokens
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with
        member this.AddApplicationAuthentication () =
            if this = null then nullArg "this"
            this.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(
                    fun options -> options.RequireHttpsMetadata <- false
                                   options.TokenValidationParameters <-
                                        TokenValidationParameters
                                            (
                                                ValidateIssuer = true,
                                                ValidIssuer = JwtOptions.Issuer,
                                                ValidateAudience = true,
                                                ValidAudience = JwtOptions.Audience,
                                                ValidateLifetime = true,
                                                IssuerSigningKey = JwtOptions.SymmetricSecurityKey,
                                                ValidateIssuerSigningKey = true
                                            )
                             )

[<AutoOpen>]
module ApplicationBuilderExtensions =
    open Microsoft.AspNetCore.Builder
    
    type IApplicationBuilder with
        member this.ConfigureApplicationCors () =
            if this = null then nullArg "this"
            this.UseCors(fun builder -> builder.AllowAnyOrigin()
                                               .AllowAnyMethod()
                                               .AllowAnyHeader() |> ignore)