namespace FSharp.ConsoleApp.Tests

open System
open Xunit
open FSharp.ConsoleApp

module ``Args facts`` = 

    [<Trait ("Module", "Args")>]
    module ``parse facts`` = 
        
        let [<Fact>] ``Arguments prefixed with a double dash are flags`` () = 

            let values = [| "--switch" |]
            let args = Args.parse values

            let switch =
                match args with
                | [ (Flag flag) ] -> flag
                | _ -> String.Empty

            Assert.Equal<String> ("switch", switch)

        let [<Fact>] ``Arguments prefixed with a dash and followed by an argument with no prefix are settings`` () = 
        
            let values = [| "-key"; "value" |]
            let args = Args.parse values

            let key, value = 
                match args with
                | [ (Setting (key, value)) ] -> key, value
                | _ -> String.Empty, String.Empty

            Assert.Equal<String> ("key", key)
            Assert.Equal<String> ("value", value)

        let [<Fact>] ``Setting values which contain dashes are correctly parsed`` () = 

            let values = [| "-key"; "a-hyphenated-value" |]
            let args = Args.parse values

            let key, value = 
                match args with
                | [ (Setting (key, value)) ] -> key, value
                | _ -> String.Empty, String.Empty

            Assert.Equal<String> ("key", key)
            Assert.Equal<String> ("a-hyphenated-value", value)

        let [<Fact>] ``Arguments prefixed with a dash but not followed by an argument with no prefix are ignored`` () = 

            let values = [| "-key" |] 
            let args = Args.parse values

            Assert.True (List.isEmpty args)

        let [<Fact>] ``Argument without prefix at the head of the input array are commands`` () =

            let values = [| "cmd"; |]
            let args = Args.parse values

            let command = 
                match args with
                | [ (Command command) ] -> command
                | _ -> String.Empty

            Assert.Equal<String> ("cmd", command)

        let [<Fact>] ``Arguments without prefix not at the head of the input array or after setting keys are ignored`` () = 

            let values = [| "--switch"; "cmd" |]
            let args = Args.parse values

            let isCorrect = 
                match args with
                | [ (Flag "switch") ] -> true
                | _ -> false

            Assert.True (isCorrect)

module ``Dispatcher facts`` = 

    [<Trait ("Module", "Dispatcher")>]
    module ``exec facts`` = 

        let [<Fact>] ``Correct handler is called when command is present in arguments`` () = 
        
            let args = [ (Command "x"); ]
            let handlers = [ ("x", fun _ -> true); ("y", fun _ -> false); ]
            let default' = fun _ -> false

            let result = Dispatcher.exec default' handlers args

            Assert.True (result)

        let [<Fact>] ``Command handler is called with remaining arguments`` () = 
        
            let args = [ (Command "x"); (Flag "switch"); ]
            let default' = fun _ -> false

            let handlers = [
                ("x", fun args' -> args' = [ (Flag "switch") ]);
                ("y", fun _ -> false);
            ]

            let result = Dispatcher.exec default' handlers args

            Assert.True (result)

        let [<Fact>] ``Default handler is called when no command is present in arguments`` () = 
        
            let args = [ (Flag "switch") ]
            let default' = fun _ -> "Default"
            let handlers = []

            let result = Dispatcher.exec default' handlers args

            Assert.Equal<String> ("Default", result)
        
        let [<Fact>] ``Default handler is called when no matching command is present in arguments`` () =
        
            let args = [ (Command "x"); (Flag "switch"); ]
            let default' = fun _ -> true

            let handlers = [
                ("y", fun _ -> false);
                ("z", fun _ -> false);
            ]

            let result = Dispatcher.exec default' handlers args

            Assert.True (result)

module ``App facts`` = 

    [<Trait ("Module", "App")>]
    module ``tryGetSetting facts`` = 

        let [<Fact>] ``Some value returned when key is present`` () = 

            let value = 
                [ Setting ("key", "value") ]
                |> App.tryGetSetting "key"
                |> Option.get

            Assert.Equal<String> ("value", value)

        let [<Fact>] ``None returned when key is not present`` () =
            
            let isNone = 
                [ Setting ("key", "value") ]
                |> App.tryGetSetting "anotherKey"
                |> Option.isNone

            Assert.True (isNone)

    [<Trait ("Module", "App")>]
    module ``isFlagged facts`` = 

        let [<Fact>] ``True when the flag is present`` () = 
           [ (Flag "switch") ]
           |> App.isFlagged "switch"
           |> Assert.True

        let [<Fact>] ``False when the flag is not present`` () = 
            [ (Flag "switch") ]
            |> App.isFlagged "anotherSwitch"
            |> Assert.False
