using System.Collections.Generic;
using System.Linq;

namespace MyLanguageInterpreter
{
    internal class Reslover : StatementVisitor<object>, ExpressionVisitor<object>
    {
        private Interpreter interpreter;
        private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();

        public Reslover(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        void beginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        void endScope() 
        {
            scopes.Pop();
        }

        void resolve(Statement statement)
        {
            statement.Accept(this);
        }

        public void resolve(List<Statement> statements)
        {
            foreach (Statement stmnt in statements) 
                stmnt.Accept(this);
        }

        void resolve(Expression expression)
        {
            expression.Accept(this);
        }

        void resolveLocal(Expression expr, Token name)
        {
            for(int i = scopes.Count - 1; i >= 0; i--)
                if (scopes.ElementAt(i).ContainsKey((string)name.value))
                {
                    interpreter.resolve(expr, scopes.Count - 1 - i);
                    return;
                }
        }

        void resolveFunction(Function func)
        {
            beginScope();
            foreach(Token param in func.parameters)
            {
                declare(param);
                define(param);
            }
            resolve(func.body);
            endScope();
        }

        void declare(Token name)
        {
            if (scopes.Count == 0) return;
            if (scopes.Peek().ContainsKey((string)name.value))
                throw new Error(name, "Resolver: Variable with this name is already delcraded in this scope.");
            scopes.Peek().Add((string)name.value, false);
        }

        void define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[(string)name.value] = true;
        }

        public object visitAnonymFuncExpr(AnonymFunc expr)
        {
            declare(expr.func.name);
            define(expr.func.name);

            resolveFunction(expr.func);
            return null;
        }

        public object visitAssignmentExpr(Assignment expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitBinaryExpr(Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right); 
            return null;
        }

        public object visitBlockStmnt(Block stmnt)
        {
            beginScope();
            resolve(stmnt.statements);
            endScope();
            return null;
        }

        public object visitBreakStmnt(Break stmnt)
        {
            return null;
        }

        public object visitCallExpr(Call expr)
        {
            resolve(expr.callee);
            foreach (Expression arg in expr.arguments)
                resolve(arg);
            return null;
        }

        public object visitExpressionStmnt(ExpressionStatement stmnt)
        {
            resolve(stmnt.expr);
            return null;
        }

        public object visitFunctionStmnt(Function stmnt)
        {
            declare(stmnt.name);
            define(stmnt.name);

            resolveFunction(stmnt);
            return null;
        }

        public object visitGroupingExpr(Grouping expr)
        {
            resolve(expr.group);
            return null;
        }

        public object visitIfStmnt(If stmnt)
        {
            resolve(stmnt.Condition);
            resolve(stmnt.Then);
            if (!(stmnt.Else is null)) resolve(stmnt.Else);
            return null;
        }

        public object visitLiteralExpr(Literal expr)
        {
            return null;
        }

        public object visitLogicalExpr(Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitPrintStmnt(Print stmnt)
        {
            resolve(stmnt.expr);
            return null;
        }

        public object visitReturnStmnt(Return stmnt)
        {
            if (!(stmnt.expr is null)) resolve(stmnt.expr);
            return null;
        }

        public object visitTernaryExpr(Ternary expr)
        {
            resolve(expr.left);
            resolve(expr.middle); 
            resolve(expr.right);
            return null;
        }

        public object visitUnaryExpr(Unary expr)
        {
            resolve(expr.right);
            return null;
        }

        public object visitVarExpr(Var expr)
        {
            if (!(scopes.Count == 0))
            {
                bool value;
                if (!(scopes.Peek().TryGetValue((string)expr.name.value, out value)))
                    return null;
                    //throw new Error(expr.name, "Resolver: Couldn't find value in variable expression.");
                if (!value) throw new Error(expr.name, "Resolver: Variable not instantiated in variable expression.");
            }
            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitVariableStmnt(Variable stmnt)
        {
            declare(stmnt.name);
            if(!(stmnt.init is null)) resolve(stmnt.init);
            define(stmnt.name);
            return null;
        }

        public object visitWhileStmnt(While stmnt)
        {
            resolve(stmnt.condition);
            resolve(stmnt.body);
            return null;
        }

        public object visitClassStmnt(Class stmnt)
        {
            declare(stmnt.name);
            define(stmnt.name);

            beginScope();
            scopes.Peek().Add("this", true);
            
            foreach (Function func in stmnt.methods)
                resolveFunction(func);
            endScope();

            return null;
        }

        public object visitGetExpr(Get expr)
        {
            resolve(expr.obj); 
            return null;
        }

        public object visitSetExpr(Set expr)
        {
            resolve(expr.value);
            resolve(expr.obj);
            return null;
        }

        public object visitThisExpr(This expr)
        {
            resolveLocal(expr, expr.keyword);
            return null;
        }
    }
}
