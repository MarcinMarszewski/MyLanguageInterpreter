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
				Lexer lexer = new Lexer();
				//try{
				StreamReader st = new StreamReader("C:\\lex\\test.txt");
                string input = st.ReadToEnd();
				st.Close();
					Console.WriteLine(input);
					Console.ReadKey();
					Console.WriteLine("Processing:...");

                    lexer.LexLine(input);
					lexer.finishLexing();

					Parser parser = new Parser(lexer.lexerList);
					List<Statement> statements = parser.parse();
					Interpreter interpreter = new Interpreter();
					Reslover reslover = new Reslover(interpreter);
					reslover.resolve(statements);
					interpreter.interpret(statements);

					Console.WriteLine();
					Console.WriteLine("Finished Processing");
					Console.ReadKey();
				/*}
				catch (Error err)
				{
					err.Log();
				}*/
				/*catch (Exception e) 
				{
					Console.WriteLine(e.Message);
				}*/
			}
		}
	}
}