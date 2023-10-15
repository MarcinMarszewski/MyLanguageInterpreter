# CSLoxInterpreter
 Interpreter for Lox language from Crafting Interpreters book written in C#
 
Lox is a simple dynamic language created for the needs of [Crafting Interpreters](http://craftinginterpreters.com) book. 
This is my attempt on creating an interpreter for it in C#.
## Language features
 - Functional: support for closures, function passing, lambda expresssions
 - Dynamic: all types are resolved at runtime
 - Cbject oriented: method declarations, no field declarations, those can only be assigned externally

There are issues with resolving `this` keyword. The problems are result of a few poor choices when making 
the project. Takeaway that I reccomend you remember is to create a throw away version and actually make a nice thing on
the second try, as well as to use git even if you think you don't really need it for this one little thing.