using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
{
   public class FileExtensionToProgrammingLanguage
    {
        private Dictionary<String,String> ProgrammingLanguages()
        {
            Dictionary<String, String> programmingLanguages = new Dictionary<string, string>();
            programmingLanguages.Add("PHP", "php");
            programmingLanguages.Add("Python", "py");
            programmingLanguages.Add("TSQL", "py");
            programmingLanguages.Add("C#", "cs");
            programmingLanguages.Add("Java", "java");
            programmingLanguages.Add("JavaScript", "js");
            programmingLanguages.Add("HTML", "html");
            programmingLanguages.Add("CSS", "css");
            programmingLanguages.Add("VB", "vb");
            return programmingLanguages;
        }
    }
}
