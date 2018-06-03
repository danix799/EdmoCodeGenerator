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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeGenerator.Helpers;
using CodeGenerator.Objects;
using Microsoft.Win32;
using DatabaseSchemaReader.DataSchema;
using Alphaleonis.Win32.Filesystem;
using Newtonsoft.Json;
using CodeGeneratorUI.Controls;

namespace CodeGeneratorUI
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        DatabaseStorageHelper SavedDatabases = new DatabaseStorageHelper();
        TemplateStorageHelper SavedTemplates = new TemplateStorageHelper();
        DBSchemaReaderHelper SchemaReader = new DBSchemaReaderHelper();

        public MainWindow()
        {
            InitializeComponent();
            LoadListDatabases();
            LoadTemplatesTreeView();

        }

        private void LoadListDatabases() {
            listDatabases.Items.Clear();
            List<Database> Databases = SavedDatabases.GetAll();
            if (Databases.Count == 0) {
                GridSectionDatabase.Children.Add(new MessageNoDatabase());
                return;
            }
            MessageNoDatabase messageNoDatabase = GridSectionDatabase.Children.OfType<MessageNoDatabase>().FirstOrDefault();
            GridSectionDatabase.Children.Remove(messageNoDatabase);

            foreach (Database db in Databases) {
                Button btn = new Button();
                btn.Style = App.Current.Resources["MaterialDesignToolButton"] as Style;               
                
                MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Database;
                icon.Style = App.Current.Resources["MaterialDesignButtonInlineIcon"] as Style;

                TextBlock text = new TextBlock();
                text.Text = db.Name;

                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                sp.Children.Add(icon);
                sp.Children.Add(text);
                btn.Content = sp;

                btn.Tag = db;

                listDatabases.Items.Add(btn);
            }
        }


        private void OnClickCreateDatabase(object sender, RoutedEventArgs e)
        {
            new windows.WinDatabase().ShowDialog();
            LoadListDatabases();
        }

        private void OnClickListDatabase(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            if (btn != null) {
                Database db = btn.Tag as Database;
                DatabaseSchema schema = SchemaReader.GetSchema(db);

                treeSchema.Items.Clear();
                TreeViewItem tviTables = new TreeViewItem();
                tviTables.Header = "Tables";
                tviTables.Tag = schema.Tables;
                treeSchema.Items.Add(tviTables);
                foreach (DatabaseTable dbTable in schema.Tables) {
                    tviTables.Items.Add(LoadTablesFromSchema(dbTable));
                }
            }

        }

        private TreeViewItem LoadTablesFromSchema(DatabaseTable dbTable)
        {
            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = dbTable.Name;
            tvi.Tag = dbTable;
            foreach (DatabaseColumn dbColumn in dbTable.Columns) {
                tvi.Items.Add(LoadColumnsFromTables(dbColumn));
            }
            return tvi;
        }
        private TreeViewItem LoadColumnsFromTables(DatabaseColumn dbColumn)
        {
            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = dbColumn.Name;
            tvi.Tag = dbColumn;
            return tvi;
        }

        private void onClickCreateTemplate(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog Dialog = new System.Windows.Forms.FolderBrowserDialog();
            Dialog.ShowDialog();
            if (Dialog.SelectedPath != null && Dialog.SelectedPath != "") {
                CodeGenerator.Objects.Template obj = new CodeGenerator.Objects.Template(Dialog.SelectedPath);
                SavedTemplates.Save(obj);
                LoadTemplatesTreeView();
            }
        }

        private void LoadTemplatesTreeView() {
            treeTemplates.Items.Clear();
            List<Template> templates = SavedTemplates.GetAll();
            
            if (templates.Count == 0)
            {
                GridSectionTemplates.Children.Add(new MessageNoTemplate());
                return;
            }
            MessageNoTemplate messageNoTemplate = GridSectionTemplates.Children.OfType<MessageNoTemplate>().FirstOrDefault();
            GridSectionTemplates.Children.Remove(messageNoTemplate);

            foreach (Template obj in templates) {                
                if (obj.IsDirectory)
                {
                    var rootDirectoryInfo = new DirectoryInfo(obj.Path);
                    treeTemplates.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
                }

            }
        }

        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name };
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Items.Add(CreateDirectoryNode(directory));

            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Items.Add(new TreeViewItem { Header = file.Name });

            return directoryNode;

        }

        private void OnClickGenerate(object sender, RoutedEventArgs e)
        {
            Database db = ((listDatabases.Items[0] as Button).Tag as Database);
            Template template = SavedTemplates.GetAll().First();
            DatabaseSchema schema = SchemaReader.GetSchema(db); 
            String OutputPath = @"C:\Users\user\Desktop\output";      

            CopyDirectory(template.Path, OutputPath);
            //System.Threading.Thread.Sleep(10);
            var insides = Directory.GetFiles(OutputPath, "*.*",
                System.IO.SearchOption.AllDirectories);  
            foreach (String path in insides) {
                String fileContents = File.ReadAllText(path);

                // start: single file
                var anom = from x in schema.Tables
                           select new
                           {
                               Name = x.Name,
                               HasAutoNumberColumn = x.HasAutoNumberColumn,
                               Columns = from y in x.Columns
                                                                                         select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey,IsForeignKey = y.IsForeignKey, Lenght = y.Length,IsAutoNumber =  y.IsAutoNumber }
                };
                DotLiquid.Template templateLiquid = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                String CompiledOutput = templateLiquid.Render(DotLiquid.Hash.FromAnonymousObject(new { tables = anom }, true));
                File.WriteAllText(path, CompiledOutput);
                // end: single file

                //start: multiple file
                foreach (var table in anom) {
                    DotLiquid.Template templateLiquidMultiple = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                    String CompiledOutputMultiple = templateLiquidMultiple.Render(DotLiquid.Hash.FromAnonymousObject(new { table = table }, true));
                    DotLiquid.Template templateLiquidFileName = DotLiquid.Template.Parse(path); // Parses and compiles the template                
                    String CompiledFileName = templateLiquidFileName.Render(DotLiquid.Hash.FromAnonymousObject(new { table = table }, true));
                    //File.Create(CompiledFileName);
                    File.WriteAllText(CompiledFileName, CompiledOutputMultiple);
                }
                //end: multiple file
                
                
                
            }
            MessageBox.Show("Generate Sucessfull");
            System.Diagnostics.Process.Start(OutputPath);
        }
        private void CopyDirectory(String SourcePath, String DestinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                System.IO.SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                System.IO.SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
        }

        private void OnClickGotoGit(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/danix799/EdmoCodeGenerator");
        }

        private void OnClickModifyDatabase(object sender, RoutedEventArgs e)
        {
            Database db = (listDatabases.SelectedItem as Button).Tag as Database;
            if (db != null) {
                (new CodeGeneratorUI.windows.WinDatabase(db)).ShowDialog();
                LoadListDatabases();
            }
        }

        private void OnClickDeleteDatabase(object sender, RoutedEventArgs e)
        {
            Database db = (listDatabases.SelectedItem as Button).Tag as Database;
            if (db != null)
            {
                SavedDatabases.Delete(db.Id);
                LoadListDatabases();
            }
        }

        private void OnClickDeleteTemplate(object sender, RoutedEventArgs e)
        {

        }
    }
}
