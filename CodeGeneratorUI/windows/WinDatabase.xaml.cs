using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CodeGenerator;
using CodeGenerator.Helpers;
using CodeGenerator.Objects;
using CodeGenerator.Storage;

namespace CodeGeneratorUI.windows
{
    /// <summary>
    /// Lógica de interacción para WinDatabase.xaml
    /// </summary>
    public partial class WinDatabase
    {
        readonly Database _dbToUpdate;
        public WinDatabase(Database dbToUpdateRequest = null)
        {
            InitializeComponent();
            DataContext = this;
            CmbProviders.ItemsSource = DbProvider.AvailableProviders();
            CmbProviders.SelectedValuePath = "ProviderDll";
            CmbProviders.DisplayMemberPath = "ProviderName";

            _dbToUpdate = dbToUpdateRequest;
            if (_dbToUpdate != null) {
                Loaded += (a,b)  => {
                    TxtName.Text = _dbToUpdate.Name;
                    CmbProviders.SelectedItem = DbProvider.FindProviderByProviderDll(_dbToUpdate.Provider);
                    TxtConnectionString.Text = _dbToUpdate.ConnectionString;
                    TxtOwner.Text = _dbToUpdate.Owner;
                };
                
            }
            
        }

        private Boolean ValidateSave() {
            BindingExpression betxtName = TxtName.GetBindingExpression(TextBox.TextProperty);
            if (betxtName != null) betxtName.UpdateSource();

            BindingExpression becmbProviders = CmbProviders.GetBindingExpression(Selector.SelectedItemProperty);
            if (becmbProviders != null) becmbProviders.UpdateSource();

            BindingExpression betxtConnectionString = TxtConnectionString.GetBindingExpression(TextBox.TextProperty);
            if (betxtConnectionString != null) betxtConnectionString.UpdateSource();

            Boolean hasErrors = Validation.GetHasError(TxtName) || Validation.GetHasError(CmbProviders) ||
                             Validation.GetHasError(TxtConnectionString);
           
            return hasErrors;        
        }

        private void OnClickSaveDatabase(object sender, RoutedEventArgs e)
        {
            try
            {
                Boolean hasErrors = ValidateSave();
                if (hasErrors)
                    return;
                
                Database db = new Database();
                if (_dbToUpdate != null)
                    db = _dbToUpdate;
                db.ConnectionString = TxtConnectionString.Text;
                db.Name = TxtName.Text;
                db.Provider = CmbProviders.SelectedValue.ToString();
                db.Owner = TxtOwner.Text;

                DbSchemaReaderHelper schema = new DbSchemaReaderHelper();
                schema.GetSchema(db);

                DatabaseStorageHelper storage = new DatabaseStorageHelper();
                if (_dbToUpdate != null)
                    storage.Update(db);
                else
                    storage.Save(db);

                MdDialog.IsOpen = true;
            }
            catch (Exception ex) {                
                 MessageBox.Show(ex.Message);
            }            

        }

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void OnSelectedProvider(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProviders.SelectedItem != null)
            {
                Provider obj = CmbProviders.SelectedItem as Provider;
                txtSuggestedConnectionString.Text = obj.SuggestedConnectionString;
                if (string.IsNullOrWhiteSpace(TxtConnectionString.Text))
                    if (obj != null) TxtConnectionString.Text = obj.SuggestedConnectionString;
            }
        }
    }
}
