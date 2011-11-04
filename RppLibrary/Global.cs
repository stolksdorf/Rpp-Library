using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RppLibrary
{
    public static class Global
    {
        public static string[] StaticFunctions = { "pop", "push", "foreach","for","init","update" };
        
        public static List<ErrorClass> Errors;

        public static string stdTokenizeString = "#-+*=^. (){}[]<>!,\t";

        public static List<string> Tokenize(string rawline, string filter)
        {
            List<string> result = new List<string>();
            string temp = "";
            foreach (char t in rawline)
            {
                if (filter.Contains(t))
                {
                    if (temp != "") result.Add(temp);
                    result.Add(t.ToString());
                    temp = "";
                }
                else
                {
                    temp += t;
                }
            }
            if (temp != "") result.Add(temp);
            return result;
        }


    }
}
