namespace FSharp.ConsoleApp

open System

///Common utility functions
[<AutoOpen>]
module private Common =

    ///True if two strings are the same, ignoring case
    let areSame (x : String) (y : String) = 
        String.Compare (x, y, true) = 0

///Union describing argument types
type Arg = 
    | Command of String
    | Flag of String
    | Setting of (String * String)
    
///Functions for working with arguments
[<RequireQualifiedAccess>]
module Args =
    
    open System.Text.RegularExpressions

    ///Union describing a command line token
    type Token =     
        | Key of (String * Boolean)
        | Value of String

    ///Functions for working with tokens
    [<AutoOpen>]
    module private Tokens =

        ///An active pattern used to match regular expressions
        let private (|RegEx|_|) pattern str = 
            match (Regex.Match (str, pattern, RegexOptions.IgnoreCase)) with
            | result when result.Success -> 
                Array.ofSeq (seq {
                    for i = 0 to (result.Groups.Count - 1) do
                        yield result.Groups.[i].Value
                }) |> Some
            | _ -> None

        ///Converts an array of command line arguments to an array of tokens
        let tokenise = 
            let tokeniseOne (str : String) = 
                match str with
                | RegEx "^--(.+)" groups -> Key (groups.[1], true)
                | RegEx "^-(.+)" groups -> Key (groups.[1], false)
                | _ -> Value str
            in Array.Parallel.map tokeniseOne >> Array.toList

    ///Parses an array of command line arguments to a list of app arguments
    let parse =
        
        ///Reads the command from the head of the input array of tokens (if present)
        let readCommand tokens = 
            match tokens with
            | Value command :: tokens' -> ([ Command command ], tokens')
            | _ -> ([], tokens)

        ///Reads the remaining (non-command) arguments from the input array of tokens
        let rec readRemainder (args, tokens) = 
            match tokens with
            | Key (key, true) :: tokens' -> 

                let args' = (Flag key) :: args
                in readRemainder (args', tokens')

            | Key (key, false) :: Value value :: tokens' -> 

                let args' = Setting (key, value) :: args
                in readRemainder (args', tokens')

            | _ :: tokens' -> readRemainder (args, tokens')
            | [] -> args

        tokenise >> readCommand >> readRemainder >> List.rev

///Functions for invoking handlers
[<RequireQualifiedAccess>]
module Dispatcher =

    ///Miscellaneous functions used by this module
    [<AutoOpen>]
    module private Misc =         
    
        ///Splits an array of app arguments into a command/arguments pair
        let private asCommandPair = function
            | Command command :: args -> (Some command, args)
            | args -> (None, args)  

        ///Attempts to get a value by key from a key/value list
        let tryGetValue key = 
            List.tryPick (function
                | (key', value) when (areSame key key') -> Some value
                | _ -> None
            )

        ///An active pattern used to match a command invocation from a list of app args
        let (|Exec|_|) args = 
            match (asCommandPair args) with
            | (Some command, args') -> Some (command, args')
            | _ -> None

    ///Executes a handler based on the command line arguments, invoking a default if no matching
    ///command is found.
    let exec default' handlers = function
        | Exec (command, args) ->
            match (tryGetValue command handlers) with
            | Some handler -> handler args
            | _ -> default' ()            
        | _ -> default' ()

///Functions and types for running a console application
[<RequireQualifiedAccess>]
module App = 

    ///Runs an application using the given handlers
    let run default' handlers = Args.parse >> Dispatcher.exec default' handlers

    ///Try and get a setting value by key from an argument list
    let tryGetSetting key = 
        List.tryPick (function
            | Setting (key', value) when (areSame key key') -> Some value
            | _ -> None
        )

    ///Get a setting value by key from an argument list
    let getSetting key = tryGetSetting key >> Option.get

    ///True if all keys provided have settings in an argument list
    let hasSettings keys args = 
        List.forall (fun key ->
            Option.isSome (tryGetSetting key args)
        ) keys

    ///True if a flag is present in an argument list
    let isFlagged flag = 
        List.exists (function
            | Flag flag' when (areSame flag flag') -> true
            | _ -> false
        )