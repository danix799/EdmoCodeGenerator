using System;
using CodeGenerator.Storage;

namespace CodeGeneratorUI
{
        public partial class App
        {
            public App() {
                new DatabaseStorageHelper();
                new TemplateStorageHelper();
                GC.Collect();
            }
        }
}                                                                                                                                                                                                                                                                                                                                                                           