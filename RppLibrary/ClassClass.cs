using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RppLibrary
{
    public class ClassClass
    {

        public Dictionary<string, string> Properties = new Dictionary<string,string> ();   //Property name and default value
        public string Name;
        public Dictionary<string, FuncClass> Functions = new Dictionary<string,FuncClass> ();
        FuncClass Constructor;

        public ClassClass(){}

        public ClassClass(string name)
        {
            Name = name;
        }

        public List<FuncClass> RenderFunctions()
        {
            List<FuncClass > result = new List<FuncClass> ();

            foreach (string key in Functions.Keys)
            {
                //Process the Constructor
                if (key == "constructor")
                    result.Add(createConstructor(Functions["constructor"]));
                else
                    result.Add(createFunction(Functions[key]));
            }
            
            //If the class wasn't provided with a constructor, throw an error
            if(!Functions .ContainsKey ("constructor"))
                Global.Errors .Add (new ErrorClass ("Class " + Name, "No constructor found for the " + Name + " class", ""));
            

           return result;
        }


        private FuncClass createFunction(FuncClass function)
        {
            function.Name = Name + "_" + function.Name;

            List<string> new_code = new List<string>();
            foreach (string raw_line in function.Code )
            {
                LineClass line = new LineClass(raw_line);
                string newline = line.rawline;

                //Loop through each token to rename localization            
                line.Tokens = Global.Tokenize(newline, Global.stdTokenizeString);
                List<string> new_tokens = new List<string>();

                bool isComment = false;
                foreach (string token in line.Tokens)
                {
                    string new_token = token;
                    if (!isComment)
                    {
                        if (token == "#") isComment = true;
                        if (Properties .ContainsKey (new_token)) new_token = Name + "_." + new_token;
                      
                    }
                    new_tokens.Add(new_token);
                }
                newline = string.Join("", new_tokens.ToArray<string>()); //re-build each new token into a string

                //add the coments back in
                newline += line.Comments;
                new_code.Add(newline);
            }

            function.Code = new_code;

            return function;
        }

        private FuncClass createConstructor(FuncClass function)
        {
            function.Name = "constructor";

            foreach (string var in Properties.Keys)           
                function.Code.Insert(1, var + "=" + Properties[var]);

            if(!function.hasReturn )
                function.Code.Insert(function.Code.Count -1, "return " + Name + "_");
            else
                Global.Errors .Add (new ErrorClass ("Class " + Name, "Class's construtor already has a return", ""));
            function.hasReturn = true;

            return createFunction(function);
        }

    }
}
