using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public static class DbProvider
    {
        private static readonly List<Provider> Providers = new List<Provider>();

        public static List<Provider> AvailableProviders()
        {
            Providers.Clear();
            Providers.Add(new Provider
            { ProviderName = "SQL Server", ProviderDll = "System.Data.SqlClient",
                SuggestedConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;" });
            Providers.Add(new Provider
            { ProviderName = "MySql", ProviderDll = "MySql.Data.MySqlClient",
                SuggestedConnectionString = "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;SslMode=none;Allow User Variables=True;" });            
            return Providers;
        }
        public static Provider FindProviderByProviderDll(String dll) {
            return Providers.First(a => a.ProviderDll == dll);
        }

    }
    public class Provider
    {
        public String ProviderName {set; get;}
        public String ProviderDll { set; get; }
        public String SuggestedConnectionString { set; get; }
    }
}
