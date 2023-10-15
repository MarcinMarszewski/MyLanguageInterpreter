using System;
using System.Collections.Generic;
using System.IO;

namespace MyLanguageInterpreter
{
	class Program
	{
		static void Main(string[] args)
		{
			while(true)
			{
				try
				{
                    Console.Write("Enter filepath: ");
                    StreamReader st = new StreamReader(Console.ReadLine());
					string input = st.ReadToEnd();
					st.Close();
					Console.WriteLine(input);
					Console.WriteLine("Executing code:...\n");

                    Lexer lexer = new Lexer();
                    lexer.LexLine(input);
					lexer.finishLexing();

					Parser parser = new Parser(lexer.lexerList);
					List<Statement> statements = parser.parse();
					Interpreter interpreter = new Interpreter();
					Reslover reslover = new Reslover(interpreter);
					reslover.resolve(statements);
					interpreter.interpret(statements);

					Console.WriteLine("\nFinished executing.");
					Console.ReadKey();
				}
				catch (Error err)
				{
					err.Log();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
		}
	}
}