using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MyLanguageInterpreter
{
	class Interpreter : ExpressionVisitor<object>, StatementVisitor<object>
	{
		readonly public Environment global = new Environment();
		Environment environment;
		Dictionary<Expression, int> locals = new Dictionary<Expression, int>();

        public Interpreter() 
		{
			environment = global;

			global.Define("clock", new Clock());
			global.Define("readln", new ReadLn());
		}

        public void interpret(List<Statement> statements)
		{
			try
			{
				foreach(Statement statement in statements)
					if(execute(statement) is ReturnInterrupt)
							return;
			}
            catch(Error err)
            {
				err.Log();
            }
		}

		public void resolve(Expression expr, int depth)
		{
			locals.Add(expr, depth);
		}

		object  lookupVariable(Token name, Expression expr)
		{
			int depth;
			if (locals.TryGetValue(expr, out depth))
				return environment.GetAt(depth, (string)name.value);
			return global.GetValue((string)name.value);
		}

		object execute(Statement statement)
        {
			return statement.Accept(this);
        }

		public object visitTernaryExpr(Ternary expr)
		{
			object left = evaluate(expr.left);
			object middle = evaluate(expr.middle);
			object right = evaluate(expr.right);
			
			switch(expr.leftOperator.type)
			{
				case TokenType.QUESTION:
					return isTruth(left) ? middle : right;
			}
			throw new Error("Incomputable expression at line:" + expr.leftOperator.line);
		}
		public object visitBinaryExpr(Binary expr)
		{
			object left = evaluate(expr.left);
			object right = evaluate(expr.right);

			switch(expr.op.type)
			{
				case TokenType.GREATER:
					if(left is double && right is double) return (double)left > (double)right;
					throw new Error("Wrong type error for operand: > at line:"+expr.op.line);
				case TokenType.GREATER_EQUALS:
					if (left is double && right is double) return (double)left >= (double)right;
					throw new Error("Wrong type error for operand: >= at line:" + expr.op.line);
				case TokenType.LESS:
					if (left is double && right is double) return (double)left < (double)right;
					throw new Error("Wrong type error for operand: < at line:" + expr.op.line);
				case TokenType.LESS_EQUALS:
					if (left is double && right is double) return (double)left <= (double)right;
					throw new Error("Wrong type error for operand: <= at line:" + expr.op.line);
				case TokenType.MINUS:
					if (left is double && right is double) return (double)left - (double)right;
					throw new Error("Wrong type error for operand: - at line:" + expr.op.line);
				case TokenType.STAR:
					if (left is double && right is double) return (double)left * (double)right;
					throw new Error("Wrong type error for operand: + at line:" + expr.op.line);
				case TokenType.RSLASH:
					if (left is double && right is double) return (double)left / (double)right;
					throw new Error("Wrong type error for operand: / at line:" + expr.op.line);
				case TokenType.PLUS:
					if (left is double && right is double) return (double)left + (double)right;
					return left.ToString() + right.ToString();
					throw new Error("Wrong type error for operand: + at line:" + expr.op.line);

				case TokenType.EQUALS_EQUALS:
					return isEqual(left,right);
				case TokenType.EXCLAMATION_EQUALS:
					return !isEqual(left,right);
			}
			throw new Error("Incomputable expression at line:"+expr.op.line);
		}

		public object visitUnaryExpr(Unary expr)
		{
			object right = evaluate(expr.right);
			switch(expr.op.type)
			{
				case TokenType.MINUS:
					if (right is double)return -(double)right;
					throw new Error("Wrong type error for operand: - at line:" + expr.op.line);
				case TokenType.EXCLAMATION:
					return !isTruth(right);
			}
			throw new Error("Incomputable expression at line:" + expr.op.line);
		}

		public object visitLiteralExpr(Literal expr)
		{
			return expr.literal;
		}

		public object visitGroupingExpr(Grouping expr)
		{
			return evaluate(expr.group);
		}

		public object evaluate(Expression expr)
		{
			return expr.Accept(this);
		}

		bool isTruth(object value)
		{
			if (value == null) return false;
			if (value is bool) return (bool)value;
			return true;
		}

		bool isEqual(object a, object b)
		{
			if (a == null && b == null) return true;
			if (a == null) return false;
			return a.Equals(b);
		}

		public object visitExpressionStmnt(ExpressionStatement stmnt)
		{
			evaluate(stmnt.expr);
			return null;
		}

		public object visitPrintStmnt(Print stmnt)
		{
			Console.WriteLine(evaluate(stmnt.expr).ToString());
			return null;
		}

        public object visitVarExpr(Var expr)
        {
			return lookupVariable(expr.name, expr);
        }

        public object visitVariableStmnt(Variable stmnt)
        {
			object val = stmnt.init == null ? null : evaluate(stmnt.init);
			environment.Define((string)stmnt.name.value,val);
			return null;
        }

        public object visitAssignmentExpr(Assignment expr)
        {
			object val = evaluate(expr.value);

            int depth;
            if (locals.TryGetValue(expr, out depth))
                environment.AssignAt(depth, (string)(expr.name.value), val);
			else
				global.Assign((string)(expr.name.value), val);

            environment.Assign((string)expr.name.value, val);
			return val;	
        }

        public object visitBlockStmnt(Block stmnt)
        {
			return executeBlock(stmnt.statements, new Environment(environment));
        }

		public object executeBlock(List<Statement> statements, Environment enviro)
        {
			Environment prev = environment;

            try
            {
				environment = enviro;
				foreach (Statement stmnt in statements)
				{
					object tmp = execute(stmnt);
					if (tmp is Interrupt) return tmp;
				}
            }finally
            {
				environment = prev;
            }
			return null;
        }

        public object visitIfStmnt(If stmnt)
        {
			if (isTruth(evaluate(stmnt.Condition))) return execute(stmnt.Then);
			else return execute(stmnt.Else);
        }

        public object visitLogicalExpr(Logical expr)
        {
            object left = evaluate(expr.left);

			if (expr.op.type == TokenType.OR)
			{
				if (isTruth(left)) return left;
			}
			else if (!isTruth(left)) return left;
			return evaluate(expr.right);
        }

        public object visitWhileStmnt(While stmnt)
        {
			while (isTruth(evaluate(stmnt.condition)))
			{
				var tmp = execute(stmnt.body);
				if (tmp is BreakInterrupt) break;
				if (tmp is ReturnInterrupt) return tmp;
			}
			return null;
        }

        public object visitBreakStmnt(Break stmnt)
        {
            return new BreakInterrupt();
        }

        public object visitCallExpr(Call expr)
        {
			object callee = evaluate(expr.callee);
			if (!(callee is Callable)) throw new Error(callee + ": is not callable");

			List<object> args = new List<object> ();
			foreach(Expression arg in expr.arguments)
				args.Add(evaluate(arg));

			Callable function = (Callable)callee;
			if (args.Count != function.arity()) throw new Error("Wrong amount of arguments for:" +callee+ " expected:"+function.arity());
			return function.call(this, args);
        }

        public object visitFunctionStmnt(Function stmnt)
        {
			stmnt.closure = environment;
			environment.Define(stmnt.name.value.ToString(), stmnt);
			return null;
        }

        public object visitReturnStmnt(Return stmnt)
        {
			return new ReturnInterrupt(stmnt.expr != null ? evaluate(stmnt.expr) : null);
        }

        public object visitAnonymFuncExpr(AnonymFunc expr)
        {
			return expr.func;
        }

        public object visitClassStmnt(Class stmnt)
        {
			environment.Define((string)stmnt.name.value, null);

			Dictionary<string, Function> methods = new Dictionary<string, Function>();
			foreach(Function meth in stmnt.methods)
			{
				Function func = new Function(meth, environment);
				methods.Add((string)meth.name.value, meth);
			}

				
			environment.Assign((string)stmnt.name.value, new LoxClass((string)stmnt.name.value, methods));
			return null;
        }

        public object visitGetExpr(Get expr)
        {
			Object obj = evaluate(expr.obj);
            if (obj is LoxInstance ob)
            {
				return ob.Get(expr.name);
            }
			throw new Error(expr.name, "Only object instances can access properties.");
        }

        public object visitSetExpr(Set expr)
        {
            object obj = evaluate(expr.obj);
			if(obj is LoxInstance ob){
				object val = evaluate(expr.value);
				ob.Set(expr.name, val);
				return val;
			}
			throw new Error(expr.name, "Fields are only accesible in object instances.");
        }

        public object visitThisExpr(This expr)
        {
			return lookupVariable(expr.keyword, expr);
        }
        object lookupThisVariable(Token name, Expression expr)
        {
            int depth;
            if (locals.TryGetValue(expr, out depth))
                return environment.GetAt(depth, (string)name.value);
            return global.GetValue((string)name.value);
        }
    }
}