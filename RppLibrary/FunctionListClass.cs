using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RppLibrary
{
    class FunctionListClass
    {

        private static Dictionary<string, FuncClass> Functions = new Dictionary<string,FuncClass> ();

        public void add(string key, FuncClass value)
        {
            if (Functions.ContainsKey(key))
                Functions[key] = value;
            else
                Functions.Add(key, value);
        }

        public FuncClass get(string key)
        {
            return Functions[key];
        }

        public bool ContainsKey(string key)
        {
            return Functions.ContainsKey(key);
        }

        public void Clear()
        {
            Functions.Clear();
        }

    }
}
