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
        Database dbToUpdate;
        public WinDatabase(Database dbToUpdateRequest = null)
        {
            InitializeComponent();
            this.cmbProviders.ItemsSource = DBProvider.AvailableProviders();
            this.cmbProviders.SelectedValuePath = "ProviderDll";
            this.cmbProviders.DisplayMemberPath = "ProviderName";

            dbToUpdate = dbToUpdateRequest;
            if (dbToUpdate != null) {
                this.txtName.Text = dbToUpdate.Name;   
                this.cmbProviders.SelectedItem = DBProvider.FindProviderByProviderDll(dbToUpdate.Provider);
                this.txtConnectionString.Text = dbToUpdate.ConnectionString;
            }
            
        }

        private void OnClickSaveDatabase(object sender, RoutedEventArgs e)
        {           
            Database db = new Database();
            if (dbToUpdate != null)
                db = dbToUpdate;
            db.ConnectionString = this.txtConnectionString.Text;
            db.Name = this.txtName.Text;
            db.Provider = cmbProviders.SelectedValue.ToString();

            DBSchemaReaderHelper schema = new DBSchemaReaderHelper();
            schema.GetSchema(db);

            DatabaseStorageHelper storage = new DatabaseStorageHelper();
            if (dbToUpdate != null)
                storage.Update(db);
            else
            storage.Save(db);

            MessageBox.Show("Database Connected and Saved Successfull");
            this.Close();

        }

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void OnSelectedProvider(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProviders.SelectedItem != null)
            {
                Provider obj = this.cmbProviders.SelectedItem as Provider;
                this.txtConnectionString.Text = obj.SuggestedConnectionString;
            }
        }
    }
}
