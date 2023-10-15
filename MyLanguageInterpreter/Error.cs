using System;

namespace MyLanguageInterpreter
{
    class Error : Exception
    {
        public static int errCount = 0;
        public string message { get; }
        public Token token { get; }
        public Error(Token token)
        {
            errCount++;
            this.token = token;
            message = "Incorrect token: " + token.type + ":" + token.value + " . At line:" + token.line;
        }

        public Error(Token token, string mes)
        {
            errCount++;
            this.token = token;
            message = "Incorrect token: " + token.type + ":" + token.value + " . At line:" + token.line +"\nError message: "+ mes;
        }

        public Error(Token token, TokenType expected)
        {
            errCount++;
            this.token = token;
            message = "Incorrect token: " + token.type + ":" + token.value + " . At line:" + token.line + " Expected: " + expected.ToString();
        }

        public Error(string message)
        {
            errCount++;
            this.message = message;
        }
        
        public void Log()
        {
            ConsoleColor tmp = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.BackgroundColor = tmp;
        }
    }

    class Interrupt { }
    class BreakInterrupt : Interrupt { }
    class ReturnInterrupt : Interrupt 
    { 
        public object val;

        public ReturnInterrupt(object val)
        {
            this.val = val;
        }
    }
}