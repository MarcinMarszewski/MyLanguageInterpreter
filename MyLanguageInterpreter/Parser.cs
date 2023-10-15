using System.CodeDom;
using System.Collections.Generic;

namespace MyLanguageInterpreter
{
	class Parser
	{
		List<Token> tokens;
		int current = 0;

		public Parser(List<Token> tokens)
		{
			this.tokens = tokens;	
		}

		//PARSING: code structure with recursive descent
		public List<Statement> parse()
        {
			List<Statement> statements = new List<Statement>();
			while(!isAtEnd())
            {
				statements.Add(declaration());
            }

			return statements;
        }

		Statement declaration()
        {
            try
            {
				return match(TokenType.VAR) ? varDeclaration() : statement();
            }
			catch(Error err)
            {
				err.Log();
				synchronize();
				return null;
            }
        }

		Statement varDeclaration()
        {
			Token name = consume(TokenType.IDENTIFIER);
			Expression initializer = match(TokenType.EQUALS) ? expression() : null;
			consume(TokenType.SEMICOLON);
			return new Variable(name, initializer);
        }

		Statement statement()
        {
			if (match(TokenType.CLASS)) return classStatement();
			if (match(TokenType.RETURN)) return returnStatement();
			if (match(TokenType.FUN)) return functionStatement();
            if (match(TokenType.BREAK)) return breakStatement();
			if (match(TokenType.FOR)) return forStatement();
			if (match(TokenType.WHILE)) return whileStatement();
			if (match(TokenType.IF)) return ifStatement();
			if (match(TokenType.PRINT)) return printStatement();
			if (match(TokenType.LBRACKET)) return new Block(block());
			return expressionStatement();
        }

		Class classStatement()
		{
			Token name = consume(TokenType.IDENTIFIER);
			consume(TokenType.LBRACKET);

			List<Function> methods = new List<Function>();
			while (!check(TokenType.RBRACKET))
			{
				methods.Add(functionStatement());
			}

			consume(TokenType.RBRACKET);
			return new Class(name, methods);
		}

		Return returnStatement()
		{
			Expression val = !check(TokenType.SEMICOLON) ? expression() : null;
			consume(TokenType.SEMICOLON);
			return new Return(val);
		}

		Function functionStatement()
		{
			Token name = consume(TokenType.IDENTIFIER);

			consume(TokenType.LPAREN);
			List<Token> args = new List<Token>();
			while (!check(TokenType.RPAREN))
			{
				do
				{
					args.Add(consume(TokenType.IDENTIFIER));
				}
				while (match(TokenType.COMMA));
			}
			consume(TokenType.RPAREN);

			consume(TokenType.LBRACKET);
			List<Statement> body = block();
			return new Function(name, args, body);
		}

		Break breakStatement()
		{
			consume(TokenType.SEMICOLON);
			return new Break();
		}

		Statement forStatement()
		{
			consume(TokenType.LPAREN);
			Statement initializer;
			if (match(TokenType.SEMICOLON)) initializer = null;
			else if (match(TokenType.VAR)) initializer = varDeclaration();
			else initializer = expressionStatement();

			Expression condition = null;
			if(!check(TokenType.SEMICOLON)) condition = expression();
			consume(TokenType.SEMICOLON);

			Expression increment = null;
			if(!check(TokenType.RPAREN)) increment = expression();
			consume(TokenType.RPAREN);

			Statement body = statement();


			if (!(increment is null)) 
				body = new Block(new List<Statement> { body, new ExpressionStatement(increment)});

			if (condition is null) condition = new Literal(true);
			body = new While(body, condition);

			if(!(initializer is null))
				body = new Block(new List<Statement> { initializer, body });

			return body;
        }

		While whileStatement()
		{
			consume(TokenType.LPAREN);
			Expression condition = expression();
			consume(TokenType.RPAREN);
			Statement body = statement();
			return new While(body, condition);
		}

		If ifStatement()
		{
			consume(TokenType.LPAREN);
			Expression condition = expression();
			consume(TokenType.RPAREN);

			Statement thenBranch = statement();
			Statement elseBranch = match(TokenType.ELSE) ? statement() : new ExpressionStatement(new Literal(null));
			return new If(thenBranch, elseBranch, condition);
		}

		Print printStatement()
        {
			Expression val = expression();
			consume(TokenType.SEMICOLON);
			return new Print(val);
        }

		List<Statement> block()
        {
			List<Statement> stmnt = new List<Statement>();
			while (!check(TokenType.RBRACKET) && !isAtEnd())
				stmnt.Add(declaration());
			consume(TokenType.RBRACKET);
			return stmnt;
        }

		ExpressionStatement expressionStatement()
        {
			Expression val = expression();
			consume(TokenType.SEMICOLON);
			return new ExpressionStatement(val);
        }

		Expression expression()
		{
			return assignment();
		}

		Expression assignment()
        {
			Expression expr = or();
			if(match(TokenType.EQUALS))
            {
				Token equals = previous();
				Expression val = assignment();

				if(expr is Var){
					Token name = ((Var)expr).name;
					return new Assignment(name, val);
                } else if(expr is Get get){
					return new Set(get.obj, get.name, val);
				}
				new Error(equals).Log();
            }
			return expr;
        }

		Expression or()
		{
			Expression expr = and();
			while(match(TokenType.OR))
			{
				Token op = previous();
				Expression right = and();
				expr = new Logical(expr,right,op);
			}
			return expr;
		}

		Expression and()
		{
			Expression expr = conditional();

			while(match(TokenType.AND))
			{
				Token op = previous();
				Expression right = conditional();
				expr = new Logical(expr, right, op);
			}
			return expr;
		}

		Expression conditional()
        {
			Expression expr = equality();

			while (match(TokenType.QUESTION))
            {
				Token leftOp = previous();
				Expression middle = equality();
				if (!match(TokenType.COLON)) throw new Error(peek());
				expr = new Ternary(expr, leftOp, middle, previous(), equality());//equality, >> match question
			}
				
			return expr;
        }

		Expression equality()
		{
			Expression expr = comparison();

			while(match(TokenType.EXCLAMATION_EQUALS,TokenType.EQUALS_EQUALS))
				expr = new Binary(expr, previous(), comparison());
			return expr;
		}

		Expression comparison()
		{
			Expression expr = term();
			while(match(TokenType.GREATER,TokenType.GREATER_EQUALS,TokenType.LESS,TokenType.LESS_EQUALS))
				expr = new Binary(expr, previous(), term());
			return expr;
		}

		Expression term()
        {
			Expression expr = factor();
			while (match(TokenType.MINUS, TokenType.PLUS))
				expr = new Binary(expr, previous(), factor());
			return expr;
        }

		Expression factor()
		{
			Expression expr = unary();
			while (match(TokenType.RSLASH, TokenType.STAR))
				expr = new Binary(expr, previous(), unary());
			return expr;
		}

		Expression unary()
        {
			if (match(TokenType.EXCLAMATION, TokenType.MINUS))
				return new Unary(previous(),unary());
			return call();
        }

		Expression call()
		{
			Expression expr = primary();
			while (true)
			{
				if (match(TokenType.LPAREN))
					expr = finishCall(expr);
				else if (match(TokenType.DOT)){
					Token name = consume(TokenType.IDENTIFIER);
					expr = new Get(expr, name);
				}
				else break;
			}
			return expr;
		}

		private Expression finishCall(Expression callee)
		{
			List<Expression> arguments = new List<Expression>();
			if (!check(TokenType.RPAREN))
				do
				{
					arguments.Add(expression());
				} while (match(TokenType.COMMA));
			Token paren = consume(TokenType.RPAREN);
			return new Call(paren,callee,arguments);
		}

		Expression primary()
        {
			if (match(TokenType.NUMBER,TokenType.STRING, TokenType.FALSE, TokenType.TRUE, TokenType.NIL)) 
				return new Literal(previous().value);

            if (match(TokenType.THIS)) return new This(previous());

            if (match(TokenType.IDENTIFIER)) return new Var(previous());

			if (match(TokenType.FUN)) return new AnonymFunc(anonymFuncStatement());

			if(match(TokenType.LPAREN))
            {
				Expression expr = expression();
				consume(TokenType.RPAREN );
				return new Grouping(expr);
            }
			throw new Error(peek());
		}

        Function anonymFuncStatement()
        {
            consume(TokenType.LPAREN);
            List<Token> args = new List<Token>();
            while (!check(TokenType.RPAREN))
            {
                do
                {
                    args.Add(consume(TokenType.IDENTIFIER));
                }
                while (match(TokenType.COMMA));
            }
            consume(TokenType.RPAREN);

            consume(TokenType.LBRACKET);
            List<Statement> body = block();
            return new Function(new Token(TokenType.IDENTIFIER,"fun",0), args, body);
        }


        //TOKEN MANAGEMENT
        Token consume(TokenType tokenType)
        {
			if (check(tokenType)) return advance();
			throw new Error(peek(), tokenType); 
        }

		bool match(params TokenType[] types)
		{
			foreach (TokenType tokenType in types)
				if (check(tokenType))
				{
					advance();
					return true;
				}
			return false;
		}

		bool check(TokenType type)
		{
			if (isAtEnd()) return false;
			return peek().type == type;
		}

		Token advance()
		{
			if (!isAtEnd()) current++;
			return previous();
		}

		bool isAtEnd()
		{
			return peek().type == TokenType.EOF;
		}

		Token peek()
		{
			return tokens[current];
		}

		Token previous()
		{
			return tokens[current - 1];
		}

		void synchronize()
        {
			advance();

			while(!isAtEnd())
            {
				if (previous().type == TokenType.SEMICOLON)
					return;

				switch(peek().type)
                {
					case TokenType.CLASS:
					case TokenType.FUN:
					case TokenType.VAR:
					case TokenType.FOR:
					case TokenType.IF:
					case TokenType.PRINT:
					case TokenType.RETURN:
						return;
                }
				advance();
            }
        }
	}
}
