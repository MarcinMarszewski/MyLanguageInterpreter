using System.Collections.Generic;

namespace MyLanguageInterpreter
{
    class Environment
    {
        public Environment enclosing;
        Dictionary<string, object> variables = new Dictionary<string, object>();

        public Environment()
        {
            this.enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            variables.Add(name, value);
        }

        public void Assign(string name, object value)
        {
            if (variables.ContainsKey(name)) { variables[name] = value; return; }
            if(!(enclosing is null)) { enclosing.Assign(name, value); return; }

            throw new Error("Variable hasn't been initialised:" + name);
        }

        public object GetValue(string name)
        {
            object tmp;
            if (variables.TryGetValue(name, out tmp)) return tmp;
            if (!(enclosing is null)) return enclosing.GetValue(name);
            throw new Error("Variable not instantiated:"+name);
        }

        public object GetAt(int depth, string name)
        {
            if (ancestor(depth).variables.TryGetValue(name, out object tmp))
                return tmp;
            throw new Error("Couldn't resolve variable: " + name);
        }

        public void AssignAt(int depth, string name, object value)
        {
            ancestor(depth).variables[name] = value;
        }

        Environment ancestor(int depth)
        {
            Environment env = this;
            for(int i =0;i< depth; i++) 
                env = env.enclosing;
            return env;
        }
    }
}