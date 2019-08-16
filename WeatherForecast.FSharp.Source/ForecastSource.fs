module ForecastSource
    open System
    open WeatherForecast.FSharp.Domain
    open WeatherForecast.FSharp.Source
    
    let private settings = Settings.GetSample()
        
    let private mapWeather (w: ForecastSourceAPI.Weather) =
        {
            Id = Unchecked.defaultof<int64>;
            ForecastTimeItemId = Unchecked.defaultof<int64>;
            Main = w.Main;
            Description = w.Description;
            Icon = sprintf (Printf.StringFormat<_> settings.IconUrlFormat) w.Icon
        }
        
    let private mapMain (m: ForecastSourceAPI.Main) =
        {
            Id = Unchecked.defaultof<int64>;
            ForecastTimeItemId = Unchecked.defaultof<int64>;
            Temp = m.Temp;
            MinTemp = m.TempMin;
            MaxTemp = m.TempMax;
            Pressure = m.Pressure;
            Humidity = int64 m.Humidity
        }
    
    let private mapWind (w: ForecastSourceAPI.Wind) =
        {
            Id = Unchecked.defaultof<int64>;
            ForecastTimeItemId = Unchecked.defaultof<int64>;
            Speed = w.Speed;
            Degree = w.Deg
        }
        
    let private mapTimeItem (r: ForecastSourceAPI.List) =
        {
            Id = Unchecked.defaultof<int64>;
            ForecastItemId = Unchecked.defaultof<int64>;
            Time = r.DtTxt.ToUniversalTime();
            Main = mapMain r.Main;
            Weather = r.Weather
                      |> Array.map mapWeather
                      |> Array.head;
            Wind = mapWind r.Wind
        }
    
    let private map (forecast: ForecastSourceAPI.Root) =
        {
            Id = Unchecked.defaultof<int64>;
            CountryCode = forecast.City.Country;
            City = forecast.City.Name;
            Updated = DateTime.UtcNow;
            Items = forecast.List
                    |> Array.groupBy (fun i -> i.DtTxt.ToUniversalTime().Date)
                    |> Array.map (fun (date, items) ->  {
                                                            Id = Unchecked.defaultof<int64>;
                                                            ForecastId = Unchecked.defaultof<int64>;
                                                            Date = date.ToUniversalTime();
                                                            TimeItems = items
                                                                        |> Array.map mapTimeItem
                                                        })
        }
    
    let getAsync apiKey location = async {
        let apiUrl = Printf.StringFormat<string -> string -> string, _> settings.ApiUrlFormat
        let! forecast = ForecastSourceAPI.AsyncLoad (sprintf apiUrl location apiKey)
        return map forecast
    }