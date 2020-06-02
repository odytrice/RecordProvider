// Learn more about F# at http://fsharp.org

open System

open RecordProvider.Provided

type ARecord = RecordProvider<Name = "ARecord">

[<EntryPoint>]
let main argv =

    let instance = ARecord()

    instance.FirstName <- "John"
    instance.LastName <- "Doe"
    instance.Age <- 13
    instance.DateOfBirth <- DateTime.Now

    printfn "%d" instance.Age
    printfn "%s" <| instance.Greet()
    0 // return an integer exit code
