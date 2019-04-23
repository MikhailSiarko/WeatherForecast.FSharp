namespace WeatherForecast.FSharp.API.Infrastructure

open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open WeatherForecast.FSharp.API.Modules
open FluentMigrator.Runner
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration

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
    static member inline AddApplicationDatabaseMigration(services: IServiceCollection, configuration: IConfiguration) =
        services.AddFluentMigratorCore().ConfigureRunner(
            fun builder ->
                builder
                    .AddSQLite()
                    .WithGlobalConnectionString(configuration.GetConnectionString("Default"))
                    .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations
                |> ignore
        )

    [<Extension>]
    static member inline ApplyMigrations(services: IServiceCollection) =
        use scope = services.BuildServiceProvider(false).CreateScope()
        let runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>()
        if runner.HasMigrationsToApplyUp() then runner.MigrateUp()

[<Extension>]
type IApplicationBuilderExtensions () =
    [<Extension>]
    static member inline ConfigureApplicationCors(builder: IApplicationBuilder) =
        builder.UseCors(
                           fun builder -> builder.AllowAnyOrigin() |> ignore
                                          builder.AllowAnyMethod() |> ignore
                                          builder.AllowAnyHeader() |> ignore
                       )