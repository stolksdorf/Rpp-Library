using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RppLibrary
{
    public class ErrorClass
    {
        public string Description;
        public string Location;
        public string Code;
        public int LineNumber;

        public ErrorClass(string location, string text, string code)
        {
            Description = text;
            Location = location;
            Code = code;           
        }
        

    }
}
