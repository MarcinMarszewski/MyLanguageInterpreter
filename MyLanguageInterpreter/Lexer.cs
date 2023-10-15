using System;
using System.Collections.Generic;

namespace MyLanguageInterpreter
{
	class Lexer
	{
		
		private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
		{
			{"true",TokenType.TRUE },
			{"false", TokenType.FALSE },
			{"and", TokenType.AND},
			{"class", TokenType.CLASS},
			{"else", TokenType.ELSE},
			{"for", TokenType.FOR},
			{"fun", TokenType.FUN},
			{"if", TokenType.IF},
			{"nil", TokenType.NIL},
			{"or", TokenType.OR},
			{"print", TokenType.PRINT},
			{"return", TokenType.RETURN},
			{"super", TokenType.SUPER},
			{"this", TokenType.THIS},
			{"var", TokenType.VAR},
			{"while", TokenType.WHILE},
			{"break", TokenType.BREAK},
        };

		public List<Token> lexerList { get; }

		int lineNumber =1;

		string processed;
		int processPosition;

		public Lexer()
        {
			lexerList = new List<Token>();
        }

		private void listAdd(TokenType code, object value, int line)
		{
			lexerList.Add(new Token(code, value, line));
		}


		

		private bool IsAtTheEnd()
		{
			return processPosition == processed.Length;
		}

		private char Advance()
		{
			if (IsAtTheEnd()) return '\0';
			return processed[processPosition++];
		}

		private char Peek()
		{
			if (IsAtTheEnd()) return '\0';
			return processed[processPosition];
		}

		private char PeekNext()
		{
			if (processPosition+1>=processed.Length) return '\0';
			return processed[processPosition+1];
		}

		private bool Match(char match)
		{
			if(IsAtTheEnd()) return false;
			if (processed[processPosition] != match) return false;

			processPosition++;
			return true;
		}

		private bool IsDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		private bool IsAlphabetic(char c)
		{
			return (c >= 'a' && c <= 'z') || (c > 'A' && c <= 'Z') || c == '_';
		}
		private bool IsAlphaNumeric(char c)
		{
			return IsAlphabetic(c) || IsDigit(c);
		}

		private void EndLine()
		{
			while (Peek() != '\n' && !IsAtTheEnd()) Advance(); 
		}

		private void Stringify()//dodac new line
		{
			int start = processPosition;
			while (!IsAtTheEnd() && Peek() != '"')
			{
				if (Peek() == '\n') lineNumber++;
				Advance();
			}
			if (IsAtTheEnd()) throw new Error("\" expected at line "+lineNumber);
			Advance();

			listAdd(TokenType.STRING, processed.Substring(start, processPosition - start -1), lineNumber);
		}

		private void Numberify()
		{
			int start = processPosition;
			while (IsDigit(Peek())) Advance();

			if (Peek() == '.' && IsDigit(PeekNext()))
			{
				Advance();

				while (IsDigit(Peek())) Advance();
			}
			double val = 0;
			double.TryParse(processed.Substring(start - 1, processPosition - start + 1), out val);
			listAdd(TokenType.NUMBER, val, lineNumber);
		}

		private void Identifierify()
		{
			int start = processPosition;
			while (IsAlphaNumeric(Peek())) Advance();

			string tokenString = processed.Substring(start - 1, processPosition - start + 1);
			TokenType tokenT;
			if (keywords.TryGetValue(tokenString, out tokenT)) listAdd(tokenT, tokenString, lineNumber);
			else listAdd(TokenType.IDENTIFIER, tokenString, lineNumber);
			
		}

		//()+-*/ //
		public void LexLine(string line)
		{
			processed = line;
			processPosition = 0;

			char c;
			while (!IsAtTheEnd())
			{
				c = Advance();
				switch(c)
				{
					case ' ':  //skip
					case '\r':
					case '\t':
						break;
					case '\n':
						lineNumber++;
						break;

					case '(': listAdd(TokenType.LPAREN,null,lineNumber); break; //implementacja pojedynczych
					case ')': listAdd(TokenType.RPAREN,null, lineNumber); break;
					case '{': listAdd(TokenType.LBRACKET, null, lineNumber); break;
					case '}': listAdd(TokenType.RBRACKET, null, lineNumber); break;
					case '+': listAdd(TokenType.PLUS,null, lineNumber); break;
					case '-': listAdd(TokenType.MINUS,null, lineNumber); break;
					case ',': listAdd(TokenType.COMMA, null, lineNumber); break;
					case '.': listAdd(TokenType.DOT, null, lineNumber); break;
					case ':': listAdd(TokenType.COLON, null, lineNumber); break;
					case ';': listAdd(TokenType.SEMICOLON, null, lineNumber); break;
					case '?': listAdd(TokenType.QUESTION, null, lineNumber); break;

					case '*': listAdd(Match('*')?TokenType.STAR_STAR:TokenType.STAR, null, lineNumber); break; //implementacja podwojnych
					case '=': listAdd(Match('=') ? TokenType.EQUALS_EQUALS : TokenType.EQUALS, null, lineNumber); break;
					case '<': listAdd(Match('=') ? TokenType.LESS_EQUALS : TokenType.LESS, null, lineNumber); break;
					case '>': listAdd(Match('=') ? TokenType.GREATER_EQUALS : TokenType.GREATER, null, lineNumber); break;
					case '!': listAdd(Match('=') ? TokenType.EXCLAMATION_EQUALS : TokenType.EXCLAMATION, null, lineNumber); break;

					case '/':
						if (Match('/')) EndLine(); //komentarze
						else listAdd(TokenType.RSLASH, null, lineNumber);
						break;

					case '"': Stringify(); break; //napisy
					
					default:
						if (IsDigit(c)) Numberify();
						else if (IsAlphabetic(c)) Identifierify();
						else throw new Error("Unexpected character: "+c);
						break;
				}
			}
		}

		public void finishLexing()
		{
            listAdd(TokenType.EOF, null, lineNumber);
        }

		public void WriteTokens()
		{
			foreach(var val in lexerList)
			{
				Console.WriteLine(val.type.ToString()+":"+val.value+ " line:"+val.line);
			}
		}

	}
}