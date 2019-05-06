namespace WeatherForecast.FSharp.API.Infrastructure

open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open WeatherForecast.FSharp.API.Modules
open Microsoft.AspNetCore.Builder

[<Extension>]
type IServiceCollectionExtensions () =
    [<Extension>]
    static member inline AddApplicationAuthentication(services: IServiceCollection) =
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

[<Extension>]
type IApplicationBuilderExtensions () =
    [<Extension>]
    static member inline ConfigureApplicationCors(builder: IApplicationBuilder) =
        builder.UseCors(
                           fun builder -> builder.AllowAnyOrigin()
                                                 .AllowAnyMethod()
                                                 .AllowAnyHeader() |> ignore
                       )