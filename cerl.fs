module cerl


[<AutoOpen>]
module Util =
    let (|Indent|) num =
        String.replicate num " "

    let prtNl() = printf "\r\n"


type Var = string

type Atom = Atom of string

// | /literal/.
// Values of this type hold the abstract value of the literal, not the
// precise string representation used. For example, @10@, @0o12@ and @0xa@
// have the same representation.
type Literal =
    | LChar of char     // ^ character literal
    | LString of string   // ^ string literal
    | LInt of int  // ^ integer literal
    | LFloat of float   // ^ floating point literal
    | LAtom of Atom      // ^ atom literal
    | LNil              // ^ empty list
    with
        static member prt indent lit =
            match lit with
            | (LAtom (Atom atm)) -> sprintf "%s'%s'" indent atm
            | x -> failwithf "%A not impl" x

type ExprList<'T> =
    | L of 'T
    | LL of List<'T> * 'T

type Const =
    | CLit of Literal
    | CTuple of List<Const>
    | CList of ExprList<Const>

type BitString<'T> = BitString of 'T * List<Exps>

and Pat =
    | PVar of Var                 // ^ variable
    | PLit of Literal             // ^ literal constant
    | PTuple of List<Pat>             // ^ tuple pattern
    | PList of ExprList<Pat>         // ^ list pattern
    | PBinary of List<BitString<Pat>>  // ^ list of bitstring patterns
    | PAlias of Alias             // ^ alias pattern

and Alias = Alias of Var * Pat

and Pats =
    | Pat of Pat    // ^ single pattern
    | Pats of List<Pat> // ^ list of patterns

and Alt = Alt of  Pats * Guard * Exps

and Guard = Guard of Exps

and TimeOut = TimeOut of Exps * Exps

and Ann<'T> =
    | Constr of 'T      // ^ core erlang construct
    | Ann of 'T * List<Const> // ^ core erlang annotated construct

and Function = Function of Atom * int
with
    static member prt ((Indent indent) as i)
                      (Function (Atom name, arity)) =
        sprintf "%s'%s'/%i" indent name arity

// | CoreErlang expression.
and Exp =
    | Var of Var                    // ^ variable
    | Lit of Literal                // ^ literal constant
    | Fun of Function               // ^ function name
    | App of Exps * List<Exps>            // ^ application
    | ModCall of (Exps * Exps) * List<Exps> // ^ module call
    | Lambda of List<Var> * Exps          // ^ lambda expression
    | Seq of Exps * Exps              // ^ sequencing
    | Let of (List<Var> * Exps) * Exps      // ^ local declaration
    | LetRec of List<FunDef> * Exps       // ^ letrec expression
    | Case of Exps * List<Ann<Alt>>        // ^ @case@ /exp/ @of@ /alts/ end
    | Tuple of List<Exps>               // ^ tuple expression
    | List of ExprList<Exps>           // ^ list expression
    | Binary of List<BitString<Exps>>    // ^ binary expression
    | Op of Atom * List<Exps>             // ^ operator application
    | Try of Exps * (List<Var> * Exps) * (List<Var> * Exps) // ^ try expression
    | Rec of List<Ann<Alt>> * TimeOut      // ^ receive expression
    | Catch of Exps                 // ^ catch expression
    with
    static member prt ((Indent indent) as i) expr =
        match expr with
        | Var v -> v
        | Lit lit ->
            Literal.prt indent lit
        | Lambda (vars, exps) ->
            let expsp = Exps.prt (i+4) exps
            let varsp = String.concat "," vars
            sprintf "%sfun (%s) ->\r\n%s" indent varsp expsp
        | ModCall ((left, right), args) ->
            let leftExp = Exps.prt 0 left
            let rightExp = Exps.prt 0 right
            let argsp = args |> List.map (Exps.prt 0) |> String.concat ","
            sprintf "%scall %s:%s\r\n(%s%s)" indent leftExp rightExp indent argsp
        | x -> failwithf "%A not implemented" x

and Exps =
    | Exp of Ann<Exp>        // ^ single expression
    | Exps of Ann<List<Ann<Exp>>> // ^ list of expressions
    with
    static member prt ((Indent indent) as i) expr =
        match expr with
        | Exp (Constr expr) ->
            Exp.prt i expr
        | _ -> failwith "not impl"

and FunDef = FunDef of Ann<Function> * Ann<Exp>
    with
    static member prt (FunDef (def, expr)) =
        match (def, expr) with
        | (Constr f, Constr e) ->
            let fp = Function.prt 0 f
            let ep = Exp.prt 4 e
            sprintf "%s =\r\n%s\r\n" fp ep
        | _ -> failwith "Ann not implemented"

and Module = Module of Atom * List<Function> * List<Atom * Const> * List<FunDef>
    with
    static member prt (Module(Atom name, funs, attribs, defs)) =
        printf "module '%s' [" name
        let indent = 11 + name.Length
        match funs with
        | f :: funs ->
            Function.prt 0 f |> printf "%s"
            for f in funs do
                printf ",\r\n"
                Function.prt indent f |> printf "%s"
        | _ -> ()
        printf "]"
        prtNl()
        printfn "    attributes []" //TODO
        for d in defs do
            FunDef.prt d |> printfn "%s"


let square =
    Module (Atom "test", [Function (Atom "square", 1)
                          Function (Atom "module_info", 1)
                          Function (Atom "module_info", 0)],
                         [],
                         [FunDef(Constr (Function (Atom "square", 1)),
                                 Constr (Lambda(["_cor0"],
                                                  Exp (Constr (ModCall ((Exp (Constr (Lit (LAtom (Atom "erlang")))),
                                                                         Exp (Constr (Lit (LAtom (Atom "*"))))),
                                                                            [Exp (Constr (Var "_cor0"))
                                                                             Exp (Constr (Var "_cor0"))]))))))]
                          )

(*                          /* *)

(*                           [{{c_var,[],{square,1}}, *)
(*                             {c_fun,[], *)
(*                                    [{c_var,[],'_cor0'}], *)
(*                                    {c_call,[], *)
(*                                            {c_literal,[],erlang}, *)
(*                                            {c_literal,[],'*'}, *)
(*                                            [{c_var,[],'_cor0'},{c_var,[],'_cor0'}]}}}, *)
(*                             */ *)
(*     ] *)
