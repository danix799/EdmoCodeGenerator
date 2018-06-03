using CodeGenerator.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CodeGeneratorUI
{
        public partial class App : Application
        {
            public App() {
                new DatabaseStorageHelper();
                new TemplateStorageHelper();
            }
        }
}                                                                                                                                                                                                                                                                                                                                                                           