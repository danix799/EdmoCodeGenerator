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
        const string LEFT_TAG_UNPROCESSING = "LEFT_TAG_UNPROCESSING";
        const string RIGHT_TAG_UNPROCESSING = "RIGHT_TAG_UNPROCESSING";
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
        public static String GetLeftTagUnprocessing()
        {
            return Read().Get(LEFT_TAG_UNPROCESSING);
        }
        
        public static String GetRightTagUnprocessing()
        {
            return Read().Get(RIGHT_TAG_UNPROCESSING);
        }

    }
}
