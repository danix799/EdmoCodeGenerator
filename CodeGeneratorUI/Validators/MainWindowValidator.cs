using CodeGeneratorUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeGeneratorUI.Validators
{
   public class MainWindowValidator
    {
        MainWindow window;

        public MainWindowValidator(MainWindow window)
        {
            this.window = window;
        }
        public void ValidateGenerate()
        {
            this._Validate_Least_One_Database();
            this._Validate_Selected_Database();           

        }
        private void _Validate_Least_One_Database()
        {
            if (window.AddedDatabases == 0)
                throw new ValidationException(Properties.Resources.you_must_add_at_least_one_database_and_one_set_of_templates_in_order_to_continue);
        }
        private void _Validate_Selected_Database()
        {
            if (!window.HasSelectedDatabase)
                throw new ValidationException(Properties.Resources.you_must_select_a_database);
        }
    }
}
