using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace RppLibrary
{
   public class FileClass
    {
       
        public List<string> rppCode = new List<string>();
        public List<string> rslCode = new List<string>();

        public Dictionary<string, FuncClass> Functions;  
        public Dictionary<string, ClassClass> Classes;

        private Dictionary<string, FuncClass> InheritedFunctions;
        private Dictionary<string, FuncClass> ExtendedFunctions;

        private FunctionListClass FunctionList = new FunctionListClass();

        //public FileClass() { }

        public FileClass(string filepath)
        {
            StreamReader SR;
            string templine;
            SR = File.OpenText(filepath);
            templine = SR.ReadLine();
            while (templine != null)
            {
                rppCode.Add(templine);
                templine = SR.ReadLine();
            }
            SR.Close();
            Initialize();
        }

        public FileClass(string[] Lines)
        {
            rppCode.AddRange(Lines);
            Initialize();
        }

        //After the rawCode is loaded, this function runs everything
        private void Initialize(){


            ////classes = getClasses(rppcode)
            ////renderClasses(classes)



            FunctionList.Clear();
            Functions = new Dictionary<string, FuncClass>();
            Classes = new Dictionary<string, ClassClass>();
            InheritedFunctions = new Dictionary<string, FuncClass>();
            ExtendedFunctions = new Dictionary<string, FuncClass>();

            ExtractCode(rppCode);
            
            //renderClasses();
            addPrototypes();
            Functions = renderFunctions();
            
            rslCode = renderRSL();
        }
       /*
        //Converts all R++ commenting ("//", "/*", and "/") to RSL style
        private List<string> convertComments(List<string> code)
        {
            List<string> result = new List<string>();
            bool blockcommentFlag = false;

            foreach (string line in code)
            {
                string newline = line;
                if (blockcommentFlag) newline = "#" + newline;
                if (newline.Contains("/*")) { newline = newline.Replace("/*", "#"); blockcommentFlag = true; }
                if (newline.Contains("/")) { newline = newline.Replace("/", ""); blockcommentFlag = false; }
                newline = newline.Replace("//", "#");
                result.Add(newline);
            }
            return result;
        }

        //Extracts all Extended and Inherited Functions from the prototypes
        private void getPrototypes(List<string> Code)
        {
            InheritedFunctions = new Dictionary<string, FuncClass>();
            ExtendedFunctions = new Dictionary<string, FuncClass>();

            foreach (string untrimmed_line in Code)
            {
                string line = untrimmed_line.Trim();

                if (line.ToLower().StartsWith("#inherit"))
                {
                    try
                    {
                        string temp_file_name = line.Replace("#inherit ", "");
                        FileClass temp_file = new FileClass(temp_file_name);
                        foreach (string key in temp_file.Functions.Keys)
                            if (!InheritedFunctions.ContainsKey(key)) InheritedFunctions.Add(key, temp_file.Functions[key]);
                    }
                    catch
                    {
                        Global.Errors.Add (new ErrorClass ("File", "Improper use of #inherit", line);
                    }
                }
                else if (line.ToLower().StartsWith("#extend"))
                {
                    try
                    {
                        string temp_file_name = line.Replace("#extend ", "");
                        FileClass temp_file = new FileClass(temp_file_name);
                        foreach (string key in temp_file.Functions.Keys)
                            if (!ExtendedFunctions.ContainsKey(key)) ExtendedFunctions.Add(key, temp_file.Functions[key]);
                    }
                    catch
                    {
                        Global.Errors.Add (new ErrorClass ("File", "Improper use of #extend", line);
                    }
                }
                else if (line != "") break;
            }
        }

        //Returns a list of all the non-converted functions within the code
        private Dictionary <string,FuncClass > getFunctions(List<string> Code)
        {
            Dictionary <string,FuncClass > result = new Dictionary<string, FuncClass>();
            FuncClass tempfunc = new FuncClass();

            bool funcFlag = false;

            foreach (string untrimmed_line in Code)
            {
                string line = untrimmed_line.Trim();

                //Collect Header
                if ((line.StartsWith("#") || line == "") && !funcFlag)
                {
                    tempfunc.Header.Add(line);
                }
                //Get Function declaration
                else if (!funcFlag)
                {
                    funcFlag = true;
                    tempfunc.getDefinition(line);
                }
                //Get code for each function
                else
                {
                    tempfunc.Code.Add(line);
                    //Function Closing
                    if (!(line.StartsWith("//") || line.StartsWith("#")) & line.EndsWith("}"))
                    {
                        if (!result.ContainsKey(tempfunc.Name))  //Duplicity Checking
                        {
                            tempfunc.Initialize();
                            result.Add(tempfunc.Name, tempfunc);
                        }
                        else Global.Errors.Add (new ErrorClass ("File", "Function name already declared", line);

                        funcFlag = false;
                        tempfunc = new FuncClass();
                    }
                }
            }
            //Adds all the declared functions to a global list used in other parts of the code
            global.addFunctions(result);

            return result;
        }*/

       //Extracts all usable code in one pass: Prototypes, block comments, and functions
       private void ExtractCode(List<string> code)
        {
            bool inFunction = false;
            bool inBlockComment = false;
            bool inClass = false;

            ClassClass temp_class = new ClassClass ();
            //Global.CurrentLine = 0;

            List<string> header = new List<string>();
            List<string> body = new List<string>();
            string defintion_line = "";

            foreach (string untrimmed_line in code)
            {
                string line = untrimmed_line.ToLower().Trim();
                //Global.CurrentLine++; //increment the line for error purposes


                //Handle Block Commenting right away
                if (inBlockComment) line = "#" + line;
                line = line.Replace("//", "#");
                if (line.Contains("/*"))
                {
                    line = line.Replace("/*", "#");
                    inBlockComment = true;
                }
                if (line.Contains("*/"))
                {
                    line = line.Replace("*/", "");
                    inBlockComment = false;
                }

                if (line.StartsWith("#inherit")) 
                    InheritedFunctions = ProcessProtoype (line.Replace ("#inherit ",""), InheritedFunctions );
                else if (line.StartsWith("#extend"))
                    ExtendedFunctions = ProcessProtoype(line.Replace("#extend ", ""), ExtendedFunctions);
                else if ((line.StartsWith("//") || line.StartsWith("#") || line == "") && !inFunction)
                    header.Add(line);


                else if (!inClass & line.StartsWith("class"))
                {
                    inClass = true;
                    temp_class = new ClassClass (line.Replace ("class ",""));
                }
                else if (inClass & !inFunction & line.Contains('='))
                {
                    temp_class.Properties.Add(line.Split('=')[0].Trim (), line.Split('=')[1].Trim());
                }
                else if (inClass & !inFunction & line.Contains("end class"))
                {
                    Classes.Add(temp_class.Name, temp_class);
                    inClass = false;
                }
                else if (!inFunction) //Grabs any non-comment, non-empty line and starts addign it to functions
                {
                    inFunction = true;
                    defintion_line = line;
                }
                else
                {
                    body.Add(line);
                    if (line.Contains("}"))
                    {
                        FuncClass temp_function = new FuncClass(defintion_line, header, body);
                        if (inClass)
                            temp_class.Functions.Add(temp_function.Name, temp_function);
                        else if (!Functions.ContainsKey(temp_function.Name)) //Duplicity Checking
                        {  
                            Functions.Add(temp_function.Name, temp_function);
                            FunctionList.add (temp_function.Name, temp_function);
                        }
                        else
                            Global.Errors.Add(new ErrorClass("File", "Function name already declared", line));

                        //Clean up to prep for grabbing the next function
                        inFunction = false;
                        header = new List<string>();
                        body = new List<string>();
                        defintion_line = "";
                    }
                }
            }
        }

       //Populates Inherited and extended function lists to be processed in addPrototype()
        private Dictionary<string, FuncClass> ProcessProtoype(string prototype, Dictionary<string, FuncClass> Group)
        {
            try
            {
                FileClass prototype_file = new FileClass(prototype);
                foreach (string key in prototype_file.Functions.Keys)
                    if (!Group.ContainsKey(key)) Group.Add(key, prototype_file.Functions[key]);
            }
            catch
            {
                Global.Errors.Add (new ErrorClass ("File", "Improper use of Prototype", prototype));
            }

            return Group;
        }

        private void renderClasses()
        {

            foreach (string key in Classes.Keys )
            {
                List<FuncClass > temp_funcs = Classes [key].RenderFunctions ();
                foreach (FuncClass temp_func in temp_funcs)
                {
                    if (!Functions.ContainsKey(temp_func.Name)) Functions.Add(temp_func.Name, temp_func);
                }
            }
        }

        //Renders all R++ code in each function into usable RSL
        private Dictionary<string, FuncClass> renderFunctions()
        {
            Dictionary<string, FuncClass> result = new Dictionary<string, FuncClass>();
            foreach (string key in Functions.Keys )
            {
                FuncClass func = Functions[key];
                func.Render();
                result.Add(func.Name, func);
            }
            return result;
        }

        //Adds in all needed functions from inherited and extended sources
        private void addPrototypes()
        {
            //add in the extended functions
            foreach (string key in ExtendedFunctions.Keys)
            {
                if (!Functions.ContainsKey(key))
                {
                    Functions.Add(key, ExtendedFunctions[key]);
                    FunctionList.add(key, ExtendedFunctions[key]);
                }
                else
                    Global.Errors.Add(new ErrorClass("File", "Unable to extend, that function already exists within the current file", key));
            }

            //Overwrite all inherited functions
            foreach (string key in InheritedFunctions.Keys)
            {
                if (Functions.ContainsKey(key))
                {
                    Functions[key] = InheritedFunctions[key];
                    FunctionList.add(key, InheritedFunctions[key]);
                }
            }
        }

        //Convert the dictionary of functions into a list of untabbed rsl code
        private List<string> renderRSL()
        {
            List<string> result = new List<string>();
            foreach (string key in Functions.Keys  )
                result .AddRange (Functions[key].renderedCode);
            return result;
        }
        
    }
}
