using System;
using CodeGenerator.Storage;

namespace CodeGeneratorUI
{
        public partial class App
        {
        
            public App() {
            System.Threading.Thread.CurrentThread.CurrentUICulture =
            new System.Globalization.CultureInfo("es");
            new DatabaseStorageHelper();
                new TemplateStorageHelper();
                GC.Collect();
            }
        }
}                                                                                                                                                                                                                                                                                                                                                                           