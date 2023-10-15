
using System.Collections.Generic;

namespace MyLanguageInterpreter
{
    internal class LoxClass : Callable
    {
        public string name;
        Dictionary<string, Function> methods;

        public LoxClass(string name, Dictionary<string, Function> methods)
        {
            this.name = name;
            this.methods = methods;
        }

        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> args)
        {
            return new LoxInstance(this);
        }

        public override string ToString()
        {
            return name;
        }

        public Function getMethod(string name)
        {
            if(methods.TryGetValue(name, out Function val)) return val;
            return null;
        }
    }

    internal class LoxInstance
    {
        LoxClass lClass;
        Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass lClass)
        {
            this.lClass = lClass;
        }

        public object Get(Token name)
        {
            if(fields.TryGetValue((string)name.value, out object value))
                return value;
            Function val = lClass.getMethod((string)name.value);
            if (val != null) return val.bind(this);
            throw new Error(name, "Undefined property: "+(string)name.value);
        }

        public object Set(Token name, object val)
        {   
            if (fields.ContainsKey((string)name.value)) fields[(string)name.value] = val;
            else fields.Add((string)name.value, val);
            return null;
        }

        public override string ToString()
        {
            return "instance: " + lClass.name;
        }
    }
}
