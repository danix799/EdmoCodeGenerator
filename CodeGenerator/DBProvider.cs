using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    public static class DBProvider
    {
        private static List<Provider> Providers = new List<Provider>();

        public static List<Provider> AvailableProviders()
        {
            Providers.Add(new Provider() { ProviderName = "SQL Server", ProviderDll = "System.Data.SqlClient", SuggestedConnectionString = "" });
            Providers.Add(new Provider() { ProviderName = "MySql", ProviderDll = "MySql.Data.MySqlClient",
                SuggestedConnectionString = "Server=SERVER_NAME_HERE;Database=DATABASE_NAME_HERE;Uid=USERNAME_HERE;Pwd=PASSWORD_HERE;SslMode=none;Allow User Variables=True;" });            
            return Providers;
        }
        public static Provider FindProviderByProviderDll(String Dll) {
            return Providers.First(a => a.ProviderDll == Dll);
        }

    }
    public class Provider
    {
        public String ProviderName {set;get;}
        public String ProviderDll { set; get; }
        public String SuggestedConnectionString { set; get; }
        public Provider(String ProviderName, String ProviderDll, String SuggestedConnectionString)
        {
            this.ProviderName = ProviderName;
            this.ProviderDll = ProviderDll;
            this.SuggestedConnectionString = SuggestedConnectionString;
        }
        public Provider() { 
        
        }
    }
}
