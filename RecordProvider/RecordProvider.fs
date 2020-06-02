namespace TestProvider.Provided

open System
open System.Reflection
open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type InternalState() =
    let _state = Dictionary<string,obj>()

    member _.GetValue<'T>(name: string) =
        if _state.ContainsKey(name) then
            unbox<'T> (_state.[name])
        else
            Unchecked.defaultof<'T>

    member _.SetValue(name: string, item: obj) =
        if not (_state.ContainsKey(name)) then
            _state.Add(name, item)
        else
            _state.[name] <- item



[<TypeProvider>]
type RecordProvider(config) as this =
    inherit TypeProviderForNamespaces(config)

    let ns = "RecordProvider.Provided"

    let asm = Assembly.GetExecutingAssembly()

    let addProperty (name: string) (propType: System.Type) (baseType: ProvidedTypeDefinition) =

        let getter (args: Quotations.Expr list) = <@@ ((%%args.[0]:obj) :?> InternalState).GetValue(name) @@>

        let setter (args: Quotations.Expr list) =
            let argValue =
                let boxMethInfo =
                    Type
                        .GetType("Microsoft.FSharp.Core.Operators, FSharp.Core")
                        .GetMethod("Box")
                        .MakeGenericMethod(propType)
                Expr.Call(boxMethInfo, [ args.[1] ])

            //Expr.Coerce(argValue, typeof<obj>)
            let stateType = typeof<InternalState>
            let methInfo = stateType.GetMethod("SetValue")

            let stateExpr = Expr.Coerce(args.[0], stateType)
            Expr.Call(stateExpr, methInfo , [ Expr.Value(name); argValue ])

        let myInstanceProperty =
            ProvidedProperty(name,
                propType,
                isStatic = false,
                getterCode = getter,
                setterCode = setter)


        baseType.AddMember myInstanceProperty
        baseType

    let addMethod (method: ProvidedMethod) (baseType: ProvidedTypeDefinition) =
        baseType.AddMember method
        baseType

    let createType name (parameters: obj[]) =

        let aString = parameters.[0] :?> string

        let myType = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>, isErased = true)

        let constructor =
            <@@
                let state = InternalState()
                state
            @@>

        let ctor = ProvidedConstructor([], invokeCode = fun _ -> constructor)
        myType.AddMember(ctor)

        let greetMethod (args: Expr list) =
            <@@
                let state = (%%args.[0]:obj) :?> InternalState
                let firstName = state.GetValue<string>("FirstName")
                let lastName = state.GetValue<string>("LastName")
                let age = state.GetValue<int>("Age")
                let dateofBirth = state.GetValue<DateTime>("DateOfBirth")

                sprintf "Hello my name is %s %s, I am %d years old. I was born %A" firstName lastName age dateofBirth
            @@>

        let greet = ProvidedMethod("Greet",[],typeof<string>,isStatic = false, invokeCode = greetMethod)

        let props = Map.ofList [
            "FirstName", typeof<string>
            "LastName", typeof<string>
            "Age", typeof<int>
            "DateOfBirth", typeof<DateTime>
        ]

        myType
        |> addProperty "FirstName"      typeof<string>
        |> addProperty "LastName"       typeof<string>
        |> addProperty "Age"            typeof<int>
        |> addProperty "DateOfBirth"    typeof<DateTime>
        |> addMethod greet


    let provider =
        ProvidedTypeDefinition(asm,ns, "RecordProvider", Some typeof<obj>)



    let parameters =
        [ ProvidedStaticParameter("Name", typeof<string>) ]

    do
        provider.DefineStaticParameters(parameters, createType)
        this.AddNamespace(ns, [ provider ])

[<assembly: TypeProviderAssembly>]
do()