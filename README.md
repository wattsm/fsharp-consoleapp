### FSharp.ConsoleApp


Framework for easily creating simple command line applications.

#### Description

The framework can be used to create a simple console application taking the form:

``myapp.exe backup -filename MyFile.xml --local``

The first argument (``backup``) is the command. This tells the application what action to perform. Arguments which start with a double dash are Boolean flags, and arguments which start with a single dash are settings and are followed by their value. These values are represented in the framework by the ``Arg`` type. 

Command handlers have the signature ``Arg list -> int`` where the return value is the exit code of the application.

The ``App.run`` function is used to create an entry point for your application. You provide the function with:
- A function which prints your application's usage (``unit -> int``).
- A list of tuples that maps commands to handlers (``(String * (Arg list -> unit)) list``).
- The command line arguments (``String array``).

The framework will then either execute the handler associated with the command or, if no match is found, the usage function.

#### Example

```fsharp
module Program = 

  module Usage = 
    let exec () = ...

  module Backup =
    let exec args = ...
    
  module Restore = 
    let exec args = ...
  
  [<EntryPoint>] 
  let main argv = 
    App.run Usage.exec [
      ("backup", Backup.exec);
      ("restore", Restore.exec);
    ] argv
```

**A working example can be found in the FSharp.ConsoleApp.Example projects.**
