using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorUI.Helpers
{
   public static class AppConfigHelper
    {
        const string LAST_SELECTED_DATABASE = "LAST_SELECTED_DATABASE";
        const string REPOSITORY_URL = "REPOSITORY_URL";
        public static NameValueCollection Read()
        {
           return ConfigurationManager.AppSettings;         
        }
        public static String GetRepositoryUrl()
        {
            return Read().Get(REPOSITORY_URL);
        }

        public static void SetLastSelectedDatabase(Guid Id)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(LAST_SELECTED_DATABASE);
            config.AppSettings.Settings.Add(LAST_SELECTED_DATABASE, Id.ToString());
            config.Save(ConfigurationSaveMode.Modified);
        }
        public static Guid GetLastSelectedDatabase()
        {
            return Guid.Parse(Read().Get(LAST_SELECTED_DATABASE));
        }
        public static bool ExistsLastSelectedDatabase()
        {
            return Read().Get(LAST_SELECTED_DATABASE) != null;
        }


    }
}
