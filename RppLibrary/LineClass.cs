using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace RppLibrary
{
    class LineClass
    {
        public string Pre;
        public string FunctionKey;
        public string Post;
        public List<string> Parameters = new List<string> ();
        public List<string> Tokens;
        public string Comments;
        public string PropertyOf;

        public string rawline;
        public bool hasFunctionCall = false;

        private FunctionListClass FunctionList = new FunctionListClass();
     //   private GlobalClass global = new GlobalClass();

        public LineClass(string line)
        {
            rawline = getComments(line);  //Cut out the comments
            Tokens = Global.Tokenize(rawline, Global.stdTokenizeString );
            line = getFunction(rawline);
            if (hasFunctionCall) getParametersAndPost(line);
            else Pre = line;
        }

        private string getComments(string line)
        {
            if (line.Contains("#"))
            {
                int index = line.IndexOf('#');
                Comments = line.Substring(index);
                return line.Substring(0, index);
            }
            return line;
        }

        private string getFunction(string line)
        {
            string result = "";

            foreach (string token in Tokens)
            {
                if (FunctionList.ContainsKey (token) || Global.StaticFunctions .Contains (token))
                {
                    FunctionKey = token;
                    hasFunctionCall = true;
                    break;
                }
            }
            if (hasFunctionCall)
            {
                int index = line.IndexOf(FunctionKey+"(");
                try
                {
                    Pre = line.Substring(0, index);
                }
                catch
                {
                    Pre = "";
                }

                //check for the function being a property of a variable
                if(Pre.EndsWith (".")){
                    string temp_string = "#-+*=^ (){}[]<>!,\t";
                    PropertyOf = "";

                    List<string> temp_tokens = Global.Tokenize (Pre, Global.stdTokenizeString );
                    for (int t1 = temp_tokens.Count - 2; t1 >= 0; t1--)
                    {
                        if (temp_string.Contains(temp_tokens[t1])) break;
                        else PropertyOf = temp_tokens[t1] + PropertyOf;
                    }
                }
                result = line.Substring(index + FunctionKey.Length);
            }
            else
                result = line;
            return result;
        }

        private void getParametersAndPost(string line)
        {
            List<string> temp_tokens = Global.Tokenize(line, "(),");
            string slush = "";
            int brac_count =1;
            temp_tokens.Remove("(");
            foreach (string token in temp_tokens)
            {
                if (brac_count != 0)
                {
                    if (token == "(") brac_count++;
                    if (token == ")") brac_count--;
                    if (token == "," & brac_count == 1)
                    {
                        Parameters.Add(slush);
                        slush = "";
                    }
                    else if(brac_count !=0) slush += token;
                }else                   
                    Post += token;
            }
            if(slush != "") Parameters .Add(slush);
            
        }
    }
}