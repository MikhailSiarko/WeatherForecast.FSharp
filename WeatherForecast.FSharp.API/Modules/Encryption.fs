namespace WeatherForecast.FSharp.API.Modules

open System
open System.Text
open System.Security.Cryptography

module Encryption =
    [<Literal>]
    let private Empty = ""

    let private salt = "ytrewQ"

    let private executeAlgorithm (source: string) =
        use sh1 = new SHA1CryptoServiceProvider()
        source.Insert(source.Length - 3, salt)
        |> Encoding.UTF8.GetBytes
        |> sh1.ComputeHash
        |> Array.fold (fun acc i -> acc + i.ToString("X4")) String.Empty

    let encrypt =
        function
        | null
        | Empty -> invalidArg "source" "Source string can't be null or empty"
        | str -> executeAlgorithm str