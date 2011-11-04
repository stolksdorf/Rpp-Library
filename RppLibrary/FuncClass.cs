using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;

namespace RppLibrary
{
    public class FuncClass
    {
        public string Name = "";
        public List<string> Header = new List<string>();
        public List<string> Code = new List<string>();
        public List<string> Parameters = new List<string>();
        public List<string> Local = new List<string>();
        public List<string> renderedCode = new List<string>();
        public bool hasReturn = false;

        private FunctionListClass FunctionList = new FunctionListClass();
        //private GlobalClass global = new GlobalClass();

        private int foreach_inc_var = 0;
        private int whileIndex = 0;
        private Hashtable whilePost = new Hashtable();

        public FuncClass() { }

        public FuncClass(string definition, List<string> header, List<string> body)
        {
            getDefinition(definition);
            Code.AddRange ( Localize(body));
            Header = header;
        }

        //Extracts the Name and Parameters from the Function Declaration
        private void getDefinition(string rawline)
        {
            //Extract the name and parameters from the rawline
            if (rawline.Contains("("))
            {
                Name = rawline.Substring(0, rawline.IndexOf("(", 0));
                if (rawline.Contains("{")) Code.Add("{");

                string temp = rawline.Remove(0, rawline.IndexOf("(") + 1);
                if (rawline.Contains(")"))
                {
                    temp = temp.Remove(temp.IndexOf(")"));
                    string[] temp_para = temp.Split(',');
                    foreach (string t0 in temp_para)
                    {
                        if (t0.Trim() != "") Parameters.Add(t0.Trim());
                    }
                }
            }
            else if (rawline.Contains("{"))
            {
                Name = rawline.Substring(0, rawline.IndexOf('{'));
                Code.Add("{");
            }
            else
                Name = rawline;
        }

        //This renders all the code within the function from R++ to RSL
        public void Render()
        {
            
            Code = Vectorize(Code);  
            Code = Normalize(Code);                      
            Code = Functionalize(Code);

            renderedCode = Merge();
        }

        //Handles all locationization and wraps up all R++ code before rendering to rsl
        private List<string> Localize(List<string> code)
        {
            List<string> result = new List<string>();

            foreach (string raw_line in code)
            {
                LineClass line = new LineClass(raw_line);
                string newline = line.rawline;

                //Handle Local
                if (newline.StartsWith("local ") & line.Tokens.Count >= 3)
                {
                    string temp = newline;
                    temp = temp.Replace("local ", "");
                    Local.Add(Global.Tokenize(temp, Global.stdTokenizeString)[0]);
                    if (temp.Contains("="))
                        newline = temp;
                    else
                        newline = "#local " + temp; //comment the localization for the rsl code
                }

                //Handle Return
                if (newline.StartsWith("return ") & line.Tokens.Count >= 3)
                {
                    newline = newline.Replace("return ", "copy(" + Name + "_.result,") + ")";
                    hasReturn = true;
                }

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
                        if (Parameters.Contains(new_token) || Local.Contains(new_token)) new_token = Name + "_." + new_token;
                    }
                    new_tokens.Add(new_token);
                }
                newline = string.Join("", new_tokens.ToArray<string>()); //re-build each new token into a string

                //add the coments back in
                newline += line.Comments;
                result.Add(newline);
            }
            return result;
        }
      
        //Converts all built-in functions into RSL code
        private List<string> Vectorize(List<string> code)
        {
            bool repeat = false;
            List<string> result = new List<string>();
            foreach (string line in code)
            {
                LineClass Line = new LineClass(line);
                if (Line.hasFunctionCall)
                {
                    repeat = true;
                    switch (Line.FunctionKey)
                    {
                        case "init":
                            result.AddRange(createInit(Line));
                            break;
                        case "update":
                            result.AddRange(createUpdate(Line));
                            break;
                        case "pop":
                            result.AddRange(createPop(Line));
                            break;
                        case "push":
                            result.AddRange(createPush(Line));
                            break;
                        case "for":
                            repeat = false;
                            result.AddRange(createFor(Line));
                            break;
                        case "foreach":
                            result.AddRange(createForeach(Line));
                            break;
                        default:
                            repeat = false;
                            result.Add(line);
                            break;
                    }
                }
                else if (Line.Tokens.Contains("while"))
                {
                    whileIndex++;
                    result.Add(line);
                }
                else if (Line.Tokens.Contains("endw") | Line.Tokens.Contains("endfor"))
                {
                    if (whilePost.ContainsKey(whileIndex))
                    {
                        repeat = true;
                        result.Add(whilePost[whileIndex].ToString());
                    }
                    whilePost.Remove(whileIndex);
                    result.Add("endw");
                    whileIndex--;
                }
                else
                    result.Add(line);
            }

            if (repeat) result = Vectorize(result);
            return result;
        }

        //Cleans up all the code and does any non-function related translation over to RSL
        private List<string> Normalize(List<string> code)
        {
            List<string> result = new List<string>();
           // bool blockcommentFlag = false;

            foreach (string raw_line in Code)
            {
                LineClass line = new LineClass(raw_line);
                string newline = line.rawline;

                //Handle Universal Copy
                if (newline.Contains('=') && !newline.Contains ("=="))
                {
                    //Handle Recursive arithmic operators
                    if (newline.Contains("+=") | newline.Contains("-=") | newline.Contains("/=") | newline.Contains("*="))
                    {
                        string[] temp1 = newline.Split('=');
                        string temp_base = temp1[0].Trim().TrimEnd(new char[] { '+', '-', '/', '*' });
                        string op = temp1[0][temp1[0].Length - 1].ToString();
                        newline = "copy(" + temp_base + "," + temp_base + op + " " + temp1[1].Trim() + ")";
                    }
                    //Handle simple assignment
                    else
                    {
                        string[] temp2 = newline.Split('=');
                        //this line converts all assignments over to deep copy
                       // newline = "copy(" + temp2[0].Trim() + "," + temp2[1].Trim() + ")";
                    }

                }
                //Handle Quick Increments
                else if (newline.Contains("++"))
                {
                    string[] temp3 = newline.Split(new string[] { "++" }, StringSplitOptions.None);
                    newline = "copy(" + temp3[0].Trim() + "," + temp3[0].Trim() + "+1)";
                }
                else if (newline.Contains("--"))
                {
                    string[] temp3 = newline.Split(new string[] { "--" }, StringSplitOptions.None);
                    newline = "copy(" + temp3[0].Trim() + "," + temp3[0].Trim() + "-1)";
                }

                //add the coments back in
                newline += line.Comments; 
                result.Add(newline);
            }
            return result;
        }

        //Generates the user function calls into RSL 
        private List<string> Functionalize(List<string> code)
        {
            bool repeat = false;
            List<string> result = new List<string> ();
            foreach (string line in code)
            {
                LineClass Line = new LineClass(line);
                if (Line.hasFunctionCall)
                {
                    if (!line.StartsWith("gosub("))
                    {
                        repeat = true;
                        result.AddRange(createFunctionCall(Line));
                    }
                    else
                        result.Add(line);   
                }
                else                
                    result.Add(line);                
            }

            if(repeat) result = Functionalize (result);
            return result; 
        }

        //Collaspes the header, name and code into a printable rsl function
        private List<string> Merge()
        {
            List<string> result = new List<string>();

            result.AddRange(Header);
            result.Add(Name);
            result.AddRange(Code);

            return result;
        }
    
        #region Create Functions

        //Add in the declared function to the global list
        private List<string> createFunctionCall(LineClass line)
        {
            FuncClass Function = FunctionList.get (line.FunctionKey);
            List<string> result = new List<string>();
            if (line.Parameters.Count == Function.Parameters.Count)
            {
                for (int t0 = 0; t0 < line.Parameters.Count; t0++)
                    result.Add("copy("+Function.Name + "_." + Function.Parameters[t0] + "," + line.Parameters[t0] + ")");
                result.Add("gosub(" + Function.Name + ")" + line.Comments );
                if (Function.hasReturn)
                    result.Add(line.Pre + Function.Name + "_.result" + line.Post);
            }
            else
                Global.Errors.Add (new ErrorClass (Name , "Invalid number of arguments in "+ Function.Name, line.rawline));
            return result;
        }

        private List<string> createInit(LineClass line)
        {
            List<string> result = new List<string>();

            result.Add("cleararray(" + line.PropertyOf + ")" + line.Comments );
            result.Add(line.PropertyOf + ".size=" + line.Parameters.Count);
            for (int t0 = 0; t0 < line.Parameters.Count; t0++)
                result.Add("pushtail(" + line.Parameters[t0] + "," + line.PropertyOf + ")");
            return result;
        }

        private List<string> createUpdate(LineClass line)
        {
            List<string> result = new List<string>();
            result.Add("countarray(" + line.PropertyOf + ")" + line.Comments );
            result.Add(line.PropertyOf + ".size=_result");
            return result ;
        }

        private List<string> createPush(LineClass line)
        {
            List<string> result = new List<string>();
            if (line.Parameters.Count == 1)
            {
                result.Add("pushhead(" + line.Parameters[0] + "," + line.PropertyOf + ")" + line.Comments );
                result.Add(line.PropertyOf + ".size++");
            }
            else
                Global.Errors.Add (new ErrorClass (Name,"Invalid number of arguments in push command",line.rawline));
            return result;
        }


        private List<string> createPop(LineClass line)
        {
            List<string> result = new List<string>();
            if (line.Parameters.Count == 0)
            {
                result.Add("poptail(" + line.PropertyOf + ".pop_result," + line.PropertyOf + ")" + line.Comments );
                result.Add(line.PropertyOf + ".size=" + line.PropertyOf + ".size-1");
                if (line.Pre != (line.PropertyOf +".") ) result.Add(line.Pre + "pop_result" + line.Post);
            }
            else
                Global.Errors.Add (new ErrorClass (Name,"Invalid number of arguments in pop command",line.rawline));
            return result;
        }

        private List<string> createFor(LineClass line)
        {
            List<string> result = new List<string>();
            if (line.Parameters.Count == 3)
            {
                result.Add(line.Parameters[0]);
                result.Add("while(" + line.Parameters[1] + ")" + line.Comments );
                whileIndex++;
                whilePost.Add(whileIndex, line.Parameters[2]);
            }
            else
                Global.Errors.Add (new ErrorClass (Name,"Invalid number of arguments in for command",line.rawline));
            return result;
        }

        private List<string> createForeach(LineClass line)
        {
            List<string> result = new List<string>();
            if (line.Parameters.Count == 2)
            {
                string param1 = "inc_var" + foreach_inc_var + " = 0,";
                string param2 = "inc_var" + foreach_inc_var + "<" + line.Parameters[1] + ".size,";
                string param3 = "inc_var" + foreach_inc_var + "++";
                result.Add("for(" + param1 + param2 + param3 + ")" + line.Comments );
                result.Add("copy(" + line.Parameters[0] + "," + line.Parameters[1] + "[" + "inc_var" + foreach_inc_var + "])");
                foreach_inc_var++;
            }
            else
                Global.Errors.Add (new ErrorClass (Name,"Invalid number of arguments in foreach command", line.rawline));
            return result;
        }

        #endregion  

    }
}

