namespace MyLanguageInterpreter
{
	public enum TokenType
	{
		PLUS, MINUS, STAR, RSLASH,                          //single char
		LBRACKET, RBRACKET, LPAREN, RPAREN,
		COMMA, DOT, COLON, SEMICOLON,
		EQUALS, LESS, GREATER, EXCLAMATION,
		QUESTION,

		STAR_STAR, EQUALS_EQUALS, EXCLAMATION_EQUALS,       //double char
		LESS_EQUALS, GREATER_EQUALS,

		TRUE, FALSE, AND, CLASS, ELSE, FOR, FUN, IF,            //keywords
		NIL, OR, PRINT, RETURN, SUPER, THIS, VAR, WHILE,
		BREAK,

		STRING, NUMBER, IDENTIFIER,                     //leximes

		EOF
	}

	class Token 
	{
        public TokenType type { get; }
		public object value { get; }

		public int line { get; }

		public Token(TokenType type, object value, int line)
		{
			this.type = type;
			this.value = value;
			this.line = line;
			if (type == TokenType.TRUE) this.value = true;
			if (type == TokenType.FALSE) this.value = false; //TODO: przenieść gdzieś indziej bo tu jest chujozaa
		}
	}
}