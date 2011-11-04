using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RppLibrary
{
    public class RppCode
    {
        public string[] RSLCode;
        public string[] Code;
        public FuncClass[] Functions;
        public ErrorClass[] Errors;
        public string Version = "0.3";

       // private GlobalClass global = new GlobalClass();

         public RppCode(string filepath)
        {
            CompilePrep();
            Compile(new FileClass(filepath));

        }

         public RppCode(string[] lines)
         {
             CompilePrep();
            Compile(new FileClass(lines));
         }

         private void Compile(FileClass temp)
         {
             Functions = temp.Functions .Values .ToArray<FuncClass>();
             RSLCode = temp.rslCode.ToArray<string>();
             Code = temp.rppCode.ToArray<string>();
             Errors = Global.Errors.ToArray();

         }

         private void CompilePrep()
         {
             Global.Errors = new List<ErrorClass>();
             //Global.Functions = new Dictionary<string, FuncClass>();
             //Global.Classes = new Dictionary<string, ClassClass>();
         }

    }
}
