namespace WeatherForecast.FSharp.API

open AutoMapper
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open WeatherForecast.FSharp.API.Infrastructure
open WeatherForecast.FSharp.API.Modules
open WeatherForecast.FSharp.API.Types
open System
open System.Reflection

type Startup (configuration: IConfiguration) =
    let apiKey = configuration.GetSection("WeatherForecastServiceApiKey").Value
    let apiUrlFormat = configuration.GetSection("APIUrlFormat").Value
    let apiUrl = Printf.StringFormat<string -> string -> string, _> apiUrlFormat
    
    let configureLoadForecast (_: IServiceProvider) = let expirationTime = configuration.GetValue<float>("ExpirationTime")
                                                      let apiLoadAsync = WeatherAPI.configureLoad apiUrl apiKey
                                                      WeatherForecast.loadAsync expirationTime apiLoadAsync

    member __.ConfigureServices(services: IServiceCollection) =
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore
        services.AddScoped<GetForecast>(Func<IServiceProvider, GetForecast>(configureLoadForecast))
            .AddApplicationAuthentication() |> ignore
        Mapper.Initialize (fun b -> b.AddMaps(Assembly.GetExecutingAssembly()))

    member __.Configure(app: IApplicationBuilder) =
        app.UseMiddleware<ExeptionHandlingMiddleware>()
            .ConfigureApplicationCors()
            .UseHttpsRedirection()
            .UseAuthentication()
            .UseMvc() |> ignore