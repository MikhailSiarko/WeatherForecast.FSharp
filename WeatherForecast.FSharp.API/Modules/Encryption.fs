namespace WeatherForecast.FSharp.API.Modules

open System.Text
open System.Security.Cryptography

module Encryption =
    [<Literal>]
    let private Empty = ""

    let private localeParameter = "ytrewQ"

    let private executeAlgorythm (source: string) =
        use sh1 = new SHA1CryptoServiceProvider()
        let bytes = Encoding.UTF8.GetBytes(source.Insert(source.Length - 3, localeParameter))
        let hash = sh1.ComputeHash(bytes)

        let sb = StringBuilder()

        Array.iter (fun (t: byte) -> sb.Append(t.ToString("X4")) |> ignore) hash

        sb.ToString()

    let encrypt =
        function
        | null
        | Empty -> invalidArg "source" "Source string can't be null or empty"
        | str -> executeAlgorythm str