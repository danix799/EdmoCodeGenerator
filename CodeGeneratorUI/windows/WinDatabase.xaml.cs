using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//my usings
using CodeGenerator.Objects;
using CodeGenerator.Helpers;
using CodeGenerator;

namespace CodeGeneratorUI.windows
{
    /// <summary>
    /// Lógica de interacción para WinDatabase.xaml
    /// </summary>
    public partial class WinDatabase : MahApps.Metro.Controls.MetroWindow
    {
        public WinDatabase()
        {
            InitializeComponent();
        }

        private void OnClickSaveDatabase(object sender, RoutedEventArgs e)
        {           
            Database db = new Database();
            db.ConnectionString = this.txtConnectionString.Text;
            db.Name = this.txtName.Text;
            db.Provider = DBProvider.MySqlClient;

            DBSchemaReaderHelper schema = new DBSchemaReaderHelper();
            schema.GetSchema(db);

            DatabaseStorageHelper storage = new DatabaseStorageHelper();
            storage.Save(db);

            MessageBox.Show("Database Connected Successfull");
            this.Close();

        }

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
