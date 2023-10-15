using System;
using System.Collections.Generic;

namespace MyLanguageInterpreter
{
    interface Callable
    {
        object call(Interpreter interpreter, List<object> args);
        int arity();
    }

    class Clock : Callable
    {
        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> args)
        {
            return (double)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }

    class ReadLn : Callable
    {
        public int arity() { return 0; }

        public object call(Interpreter interpreter, List<object> args)
        {
            return Console.ReadLine();
        }
    }
}
