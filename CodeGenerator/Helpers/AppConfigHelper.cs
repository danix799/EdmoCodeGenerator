using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
{
   public static class AppConfigHelper
    {
        const string TEMPLATES_STORAGE_FILE = "TEMPLATES_STORAGE_FILE";
        const string DATABASES_STORAGE_FILE = "DATABASES_STORAGE_FILE";
        public static NameValueCollection Read()
        {
           return ConfigurationManager.AppSettings;         
        }       
        public static String GetTemplatesStorageFile()
        {
            return Read().Get(TEMPLATES_STORAGE_FILE);
        }
        public static String GetDatabaseStorageFile()
        {
            return Read().Get(DATABASES_STORAGE_FILE);
        }

    }
}
