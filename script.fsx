open System
open FSharp.Quotations
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Reflection

let rec eval = function
    | Value(v,t) -> v
    | Coerce(e,t) -> eval e
    | NewObject(ci,args) -> ci.Invoke(evalAll args)
    | NewArray(t,args) ->
        let array = Array.CreateInstance(t, args.Length)
        args |> List.iteri (fun i arg -> array.SetValue(eval arg, i))
        box array
    | NewUnionCase(case,args) -> FSharpValue.MakeUnion(case, evalAll args)
    | NewRecord(t,args) -> FSharpValue.MakeRecord(t, evalAll args)
    | NewTuple(args) ->
        let t = FSharpType.MakeTupleType [|for arg in args -> arg.Type|]
        FSharpValue.MakeTuple(evalAll args, t)
    | FieldGet(Some(Value(v,_)),fi) -> fi.GetValue(v)
    | PropertyGet(None, pi, args) -> pi.GetValue(null, evalAll args)
    | PropertyGet(Some(x),pi,args) -> pi.GetValue(eval x, evalAll args)
    | Call(None,mi,args) -> mi.Invoke(null, evalAll args)
    | Call(Some(x),mi,args) -> mi.Invoke(eval x, evalAll args)
    | arg -> raise <| NotSupportedException(arg.ToString())
and evalAll args = [|for arg in args -> eval arg|]


let arg = 1
let exp1 = <@@ arg @@>

//let z = <@@ box (%%exp1:int) @@>

eval exp1



// let add a b = a + b

// let expr =
//     <@@
//         let firstNumber = 10
//         let secondNumber = 2
//         add firstNumber secondNumber
//     @@>

// let addX x =
//     <@@ add %%x @@>


// addX <@@ 10 @@>

// let literalNotation =
//     <@@ fun x -> System.Math.Cos x @@>

// let classNotation =
//     let var = Var("x", typeof<float>)
//     Expr.Lambda(
//         var,
//         Expr.Call(typeof<System.Math>.GetMethod("Cos"), [Expr.Var(var)]))


// let props = [
//     "FirstName"
//     "LastName"
//     "Other"
// ]

// let propExpr = Expr.NewArray(typeof<string>, props |> List.map (Expr.Value))

// let setter (args: Quotations.Expr list) = <@@ ((%%args.[0]:obj) :?> Dictionary<string,obj>).["name"] <- box(%%args.[1]:int) @@>


// let itemProp = (typeof<Dictionary<string,obj>>).GetProperty("Item")

// let setter' (args: Quotations.Expr list) =
//     let dictExpr = <@@ ((%%args.[0]:obj) :?> Dictionary<string,obj>) @@>
//     let indexerArgs = [ args.[1]]
//     let argValue = Expr.Coerce(args.[1], typeof<Int32>)
//     Expr.PropertySet(dictExpr, itemProp, args.[1], [Expr.Value("Item")])

// setter([Expr.Value(obj()); Expr.Value(100)])
// setter'([Expr.Value(obj()); Expr.Value(100)])


// //let a = Expr.Call(Expr.None)


// printfn "%A" m