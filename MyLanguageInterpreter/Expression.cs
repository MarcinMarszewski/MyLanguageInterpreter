using System.Collections.Generic;
using System.Net.Http.Headers;

namespace MyLanguageInterpreter
{
	abstract class Expression
	{
		internal abstract T Accept<T>(ExpressionVisitor<T> visitor);
	}

	interface ExpressionVisitor<T>
	{
		T visitBinaryExpr(Binary expr);
		T visitGroupingExpr(Grouping expr);
		T visitUnaryExpr(Unary expr);
		T visitLiteralExpr(Literal expr);
		T visitTernaryExpr(Ternary expr);
		T visitVarExpr(Var expr);
		T visitAssignmentExpr(Assignment expr);
		T visitLogicalExpr(Logical expr);
		T visitCallExpr(Call expr);
		T visitAnonymFuncExpr(AnonymFunc expr);
		T visitGetExpr(Get expr);
        T visitSetExpr(Set expr);
		T visitThisExpr(This expr);
    }

	class This : Expression
	{
		public Token keyword;

        public This(Token keyword)
        {
            this.keyword = keyword;
        }

        internal override T Accept<T>(ExpressionVisitor<T> visitor)
        {
            return visitor.visitThisExpr(this);
        }
    }

    class AnonymFunc : Expression, Callable
    {
		public Function func;

        public AnonymFunc(Function func)
        {
			this.func = func;
        }

        public int arity()
        {
            return func.arity();
        }

        public object call(Interpreter interpreter, List<object> args)
        {
			return func.call(interpreter, args);
        }

        internal override T Accept<T>(ExpressionVisitor<T> visitor)
        {
			return visitor.visitAnonymFuncExpr(this);
        }
    }

    class Logical : Expression
	{
		public Expression left;
		public Expression right;
		public Token op;

		public Logical(Expression left, Expression right, Token op)
		{
			this.left = left;
			this.right = right;
			this.op = op;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitLogicalExpr(this);
		}
	}

	class Ternary : Expression
	{
		public Expression left { get; }
		public Token leftOperator { get; }
		public Expression middle { get; }
		public Token rightOperator { get; }
		public Expression right { get; }


		public Ternary(Expression left, Token leftOperator, Expression middle, Token rightOperator, Expression right)
		{
			this.left = left;
			this.leftOperator = leftOperator;
			this.middle = middle;
			this.rightOperator = rightOperator;
			this.right = right;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitTernaryExpr(this);
		}
	}

	class Binary : Expression
	{
		public Expression left { get; }
		public Token op { get; }
		public Expression right { get; }

		public Binary(Expression left, Token op, Expression right)
		{
			this.left = left;
			this.op = op;
			this.right = right;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitBinaryExpr(this);
		}
	}

	class Unary : Expression
	{
		public Token op { get; }
		public Expression right { get; }

		public Unary(Token op, Expression right)
		{
			this.op = op;
			this.right = right;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitUnaryExpr(this);
		}
	}

	class Grouping : Expression
	{
		public Expression group { get; }

		public Grouping(Expression group)
		{
			this.group = group;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitGroupingExpr(this);
		}
	}

	class Literal : Expression
	{
		public object literal { get; }

		public Literal(object literal)
		{
			this.literal = literal;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitLiteralExpr(this);
		}
	}

	class Var : Expression
	{
		public Token name;

		public Var(Token name)
		{
			this.name = name;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitVarExpr(this);
		}
	}

	class Assignment : Expression
	{
		public Token name;
		public Expression value;

		public Assignment(Token name, Expression value)
		{
			this.name = name;
			this.value = value;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitAssignmentExpr(this);
		}
	}

	class Call : Expression
	{
		public Token parenthesis;
		public Expression callee;
		public List<Expression> arguments;

		public Call(Token parenthesis, Expression callee, List<Expression> arguments)
		{
			this.parenthesis = parenthesis;
			this.callee = callee;
			this.arguments = arguments;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
			return visitor.visitCallExpr(this);
		}
	}


	interface StatementVisitor<T>
	{
		T visitExpressionStmnt(ExpressionStatement stmnt);
		T visitPrintStmnt(Print stmnt);
		T visitVariableStmnt(Variable stmnt);
		T visitBlockStmnt(Block stmnt);
		T visitIfStmnt(If stmnt);
		T visitWhileStmnt(While stmnt);
		T visitBreakStmnt(Break stmnt);
		T visitFunctionStmnt(Function stmnt);
		T visitReturnStmnt(Return stmnt);
		T visitClassStmnt(Class stmnt);
	}

	abstract class Statement
	{
		internal abstract T Accept<T>(StatementVisitor<T> visitor);
	}

	class ExpressionStatement : Statement
	{
		public Expression expr;

		public ExpressionStatement(Expression expr)
		{
			this.expr = expr;
		}

		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return visitor.visitExpressionStmnt(this);
		}
	}

	class Print : Statement
	{
		public Expression expr;

		public Print(Expression expr)
		{
			this.expr = expr;
		}

		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return visitor.visitPrintStmnt(this);
		}
	}

	class Variable : Statement
	{
		public Token name;
		public Expression init;

		public Variable(Token name, Expression init)
		{
			this.name = name;
			this.init = init;
		}

		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return visitor.visitVariableStmnt(this);
		}
	}

	class Block : Statement
	{
		public List<Statement> statements;

		public Block(List<Statement> statements)
		{
			this.statements = statements;
		}

		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return visitor.visitBlockStmnt(this);
		}
	}

	class If : Statement
	{
		public Statement Then;
		public Statement Else;
		public Expression Condition;

		public If(Statement then, Statement @else, Expression condition)
		{
			Then = then;
			Else = @else;
			Condition = condition;
		}

		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return  visitor.visitIfStmnt(this);
		}
	}

	class While : Statement
	{
		public Statement body;
		public Expression condition;

		public While(Statement body, Expression condition)
		{
			this.body = body;
			this.condition = condition;
		}

		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return visitor.visitWhileStmnt(this);
		}
	}

	class Break : Statement
	{
		internal override T Accept<T>(StatementVisitor<T> visitor)
		{
			return visitor.visitBreakStmnt(this);
		}
	}

    class Function : Statement, Callable
    {
		public Token name;
		public List<Token> parameters;
		public List<Statement> body;
		public Environment closure;

        public Function(Token name, List<Token> parameters, List<Statement> body)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
        }

		public Function(Function func, Environment closure)
		{
			this.closure = closure;
			name = func.name;
			parameters = func.parameters;
			body = func.body;
		}

		public Function bind(LoxInstance instance)
		{
			Environment environment = new Environment(closure);
			environment.Define("this", instance);
			return new Function(this, environment);
		}

        public int arity()
        {
            return parameters.Count;
        }

        public object call(Interpreter interpreter, List<object> args)
        {
			Environment environment = new Environment(closure);
			for (int i = 0; i < parameters.Count; i++)
				environment.Define(parameters[i].value.ToString(), args[i]);

			var tmp = interpreter.executeBlock(body, environment);
			if (tmp is ReturnInterrupt) return ((ReturnInterrupt)tmp).val;
			return null;
        }

        internal override T Accept<T>(StatementVisitor<T> visitor)
        {
            return visitor.visitFunctionStmnt(this);
        }
    }

	class Return : Statement
	{
		public Expression expr;

        public Return(Expression expr)
        {
            this.expr = expr;
        }

        internal override T Accept<T>(StatementVisitor<T> visitor)
        {
            return visitor.visitReturnStmnt(this);
        }
    }

	class Class : Statement
	{
		public Token name;
		public List<Function> methods;

        public Class(Token name, List<Function> methods)
        {
            this.name = name;
            this.methods = methods;
        }

        internal override T Accept<T>(StatementVisitor<T> visitor)
        {
			return visitor.visitClassStmnt(this);
        }
    }

	class Get : Expression
	{
		public Expression obj;
		public Token name;

        public Get(Expression obj, Token name)
        {
            this.obj = obj;
            this.name = name;
        }

        internal override T Accept<T>(ExpressionVisitor<T> visitor)
        {
            return visitor.visitGetExpr(this);
        }
    }

    class Set : Expression
    {
        public Expression obj;
        public Token name;
		public Expression value;

        public Set(Expression obj, Token name, Expression value)
        {
            this.obj = obj;
            this.name = name;
			this.value = value;
        }

        internal override T Accept<T>(ExpressionVisitor<T> visitor)
        {
            return visitor.visitSetExpr(this);
        }
    }
}