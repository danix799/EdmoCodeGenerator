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
            this.DataContext = this;
            this.cmbProviders.ItemsSource = DBProvider.AvailableProviders();
            this.cmbProviders.SelectedValuePath = "ProviderDll";
            this.cmbProviders.DisplayMemberPath = "ProviderName";

            dbToUpdate = dbToUpdateRequest;
            if (dbToUpdate != null) {
                this.txtName.Text = dbToUpdate.Name;   
                this.cmbProviders.SelectedItem = DBProvider.FindProviderByProviderDll(dbToUpdate.Provider);
                this.txtConnectionString.Text = dbToUpdate.ConnectionString;
                this.txtOwner.Text = dbToUpdate.Owner;
            }
            
        }

        private Boolean ValidateSave() {
            Boolean HasErrors = false;

            BindingExpression betxtName = this.txtName.GetBindingExpression(TextBox.TextProperty);
            betxtName.UpdateSource();

            BindingExpression becmbProviders = this.cmbProviders.GetBindingExpression(ComboBox.SelectedItemProperty);
            becmbProviders.UpdateSource();

            BindingExpression betxtConnectionString = this.txtConnectionString.GetBindingExpression(TextBox.TextProperty);
            betxtConnectionString.UpdateSource();

            HasErrors = Validation.GetHasError(this.txtName) && Validation.GetHasError(this.cmbProviders) &&
                Validation.GetHasError(this.txtConnectionString);

           
            return HasErrors;
        
        }

        private void OnClickSaveDatabase(object sender, RoutedEventArgs e)
        {
            try
            {
                Boolean HasErrors = this.ValidateSave();
                if (HasErrors)
                    return;

                Database db = new Database();
                if (dbToUpdate != null)
                    db = dbToUpdate;
                db.ConnectionString = this.txtConnectionString.Text;
                db.Name = this.txtName.Text;
                db.Provider = cmbProviders.SelectedValue.ToString();
                db.Owner = this.txtOwner.Text;

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
            catch (Exception ex) {
                 MessageBox.Show(ex.Message);
            }

            

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
