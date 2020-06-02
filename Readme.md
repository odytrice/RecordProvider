## Record Type Provider

This simple type provider generates Records with Getters and Setters. This isn't a real type provider but it's a simple project that shows how to create types using a Type Provider

### Usage

A Simple Way to use the record is shown below

```fsharp
// Learn more about F# at http://fsharp.org

open System

open RecordProvider.Provided

type ARecord = RecordProvider<Name = "ARecord">

[<EntryPoint>]
let main argv =

    let instance = ARecord()

    instance.FirstName <- "Ody"
    instance.LastName <- "Mbegbu"
    instance.Age <- 34
    instance.DateOfBirth <- DateTime.Now

    printfn "%d" instance.Age
    printfn "%s" <| instance.Greet()
    0 // return an integer exit code

```