using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using CodeGenerator.Helpers;
using CodeGenerator.Objects;
using CodeGenerator.Storage;
using CodeGeneratorUI.Controls;
using CodeGeneratorUI.windows;
using CodeGeneratorUI.Windows;
using DatabaseSchemaReader.DataSchema;
using DotLiquid;
using MaterialDesignThemes.Wpf;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using Template = CodeGenerator.Objects.Template;
using TreeView = System.Windows.Controls.TreeView;
using CodeGeneratorUI.Helpers;
using CodeGeneratorUI.Validators;

namespace CodeGeneratorUI
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly DatabaseStorageHelper _savedDatabases = new DatabaseStorageHelper();
        readonly TemplateStorageHelper _savedTemplates = new TemplateStorageHelper();
        readonly DbSchemaReaderHelper _schemaReader = new DbSchemaReaderHelper();
        public Int32 AddedDatabases { get { return this.ListDatabases.Items.Count; } }
        public Int32 AddedTemplates { get { return this.TreeTemplates.Items.Count; } }
        public Boolean HasSelectedDatabase { get { return this.ListDatabases.SelectedItem != null; } }

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                LoadListDatabases();
                LoadTemplatesTreeView();
            }
            catch (ValidationException exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void LoadListDatabases()
        {
            ListDatabases.Items.Clear();
            List<Database> databases = _savedDatabases.GetAll();
            foreach (Database db in databases)
                ListDatabases.Items.Add(GetDatabaseUIItem(db));
            this.ToogleMessageNoDatabase();
        }

        private UIElement GetDatabaseUIItem(Database DatabaseSource)
        {
            PackIcon icon = new PackIcon
            {
                Kind = PackIconKind.Database,
                Style = Application.Current.Resources["MaterialDesignButtonInlineIcon"] as Style
            };
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(icon);
            sp.Children.Add(new TextBlock { Text = DatabaseSource.Name });
            return new Button { Style = Application.Current.Resources["MaterialDesignToolButton"] as Style, Content = sp, Tag = DatabaseSource };
        }

        private void ToogleMessageNoDatabase()
        {
            if (this.AddedDatabases == 0)
            {
                GridSectionDatabase.Children.Add(new MessageNoDatabase());
                return;
            }
            MessageNoDatabase messageNoDatabase = GridSectionDatabase.Children.OfType<MessageNoDatabase>().FirstOrDefault();
            GridSectionDatabase.Children.Remove(messageNoDatabase);
        }

        private void OnClickCreateDatabase(object sender, RoutedEventArgs e)
        {
            new WinDatabase().ShowDialog();
            LoadListDatabases();
        }

        private void OnClickListDatabase(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button) return;
            Database db = (e.Source as Button).Tag as Database;
            DatabaseSchema schema = _schemaReader.GetSchema(db);
            TreeSchema.Items.Clear();
            this.LoadTablesFromDatabase(schema);
        }

        private void LoadTablesFromDatabase(DatabaseSchema Schema)
        {
            TreeViewItem tviTables = new TreeViewItem
            {
                Header = Properties.Resources.tables,
                Tag = Schema.Tables
            };
            TreeSchema.Items.Add(tviTables);
            foreach (DatabaseTable dbTable in Schema.Tables)
                tviTables.Items.Add(LoadTablesFromSchema(dbTable));
        }

        private TreeViewItem LoadTablesFromSchema(DatabaseTable dbTable)
        {
            TreeViewItem tvi = new TreeViewItem
            {
                Header = dbTable.Name,
                Tag = dbTable
            };
            foreach (DatabaseColumn dbColumn in dbTable.Columns)
                tvi.Items.Add(LoadColumnsFromTables(dbColumn));
            return tvi;
        }
        private TreeViewItem LoadColumnsFromTables(DatabaseColumn dbColumn)
        {
            return new TreeViewItem
            {
                Header = dbColumn.Name,
                Tag = dbColumn
            };
        }

        private void OnClickCreateTemplate(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.SelectedPath))
            {
                Template obj = new Template(dialog.SelectedPath);
                _savedTemplates.Save(obj);
                LoadTemplatesTreeView();
            }
        }

        private void LoadTemplatesTreeView()
        {
            TreeTemplates.Items.Clear();
            List<Template> templates = _savedTemplates.GetAll();
            ToogleMessageNoTemplates();
            foreach (Template obj in templates)
                TreeTemplates.Items.Add(CreateTemplateNode(obj));
            ToogleMessageNoTemplates();
        }
        private void ToogleMessageNoTemplates()
        {
            if (AddedTemplates == 0)
            {
                GridSectionTemplates.Children.Add(new MessageNoTemplate());
                return;
            }
            MessageNoTemplate messageNoTemplate = GridSectionTemplates.Children.OfType<MessageNoTemplate>().FirstOrDefault();
            GridSectionTemplates.Children.Remove(messageNoTemplate);
        }

        private TreeViewItem CreateTemplateNode(Template TemplateItem)
        {
            TreeViewItem directoryNode = new TreeViewItem { Header = TemplateItem.Name, Tag = TemplateItem };
            foreach (Template directory in TemplateItem.GetChilds())
                directoryNode.Items.Add(CreateTemplateNode(directory));
            return directoryNode;
        }

        private void DeleteFolderContent(String Path)
        {
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists) return;

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void OnClickGenerate(object sender, RoutedEventArgs e)
        {
            try
            {
                (new MainWindowValidator(this)).ValidateGenerate();
                Button button = ListDatabases.Items[0] as Button;
                if (button == null) return;
                Database db = (button.Tag as Database);
                Template template = _savedTemplates.GetAll().First();
                DatabaseSchema schema = _schemaReader.GetSchema(db);
                String outputPath = @"C:\Users\user-pc\Desktop\output"; //TODO: generate for all output paths

                DeleteFolderContent(outputPath);
                CopyDirectory(template.Path, outputPath);
                //System.Threading.Thread.Sleep(10);
                var insides = Directory.GetFiles(outputPath, "*.*",
                    SearchOption.AllDirectories);
                foreach (String path in insides)
                {
                    String fileContents = File.ReadAllText(path, System.Text.Encoding.Default);

                    var anom = from x in schema.Tables
                               select new
                               {
                                   Name = x.Name,
                                   HasAutoNumberColumn = x.HasAutoNumberColumn,
                                   PrimaryKeyCount = x.PrimaryKey.Columns.Count,
                                   PrimaryKeyFirstName = x.PrimaryKey.Columns.First(),
                                   PrimaryKeyColumns = from y in x.Columns
                                                       where y.IsPrimaryKey
                                                       select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber },
                                   AutoNumberColumns = from y in x.Columns
                                                       where y.IsAutoNumber
                                                       select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber },
                                   ForeignKeyColumns = from y in x.Columns
                                                       where y.IsForeignKey
                                                       select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber },
                                   NonAutoNumberColumns = from y in x.Columns
                                                          where !y.IsAutoNumber
                                                          select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber },
                                   NonPrimaryKeyColumns = from y in x.Columns
                                                          where !y.IsPrimaryKey
                                                          select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber },
                                   NonForeignKeyColumns = from y in x.Columns
                                                          where !y.IsForeignKey
                                                          select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber },
                                   Columns = from y in x.Columns
                                             select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber }
                               };




                    // start: single file
                    //DotLiquid.Template templateLiquid = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                    //String compiledOutput = templateLiquid.Render(Hash.FromAnonymousObject(new { tables = anom }, true));
                    //compiledOutput = compiledOutput.Replace(@"'}'}'", "}}");
                    //compiledOutput = compiledOutput.Replace(@"'{'{'", "{{");
                    //File.WriteAllText(path, compiledOutput);
                    // end: single file

                    //start: multiple file
                    foreach (var table in anom)
                    {
                        DotLiquid.Template templateLiquidMultiple = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                        String compiledOutputMultiple = templateLiquidMultiple.Render(Hash.FromAnonymousObject(new { table = table }, true));
                        DotLiquid.Template templateLiquidFileName = DotLiquid.Template.Parse(path); // Parses and compiles the template                
                        String compiledFileName = templateLiquidFileName.Render(Hash.FromAnonymousObject(new { table = table }, true));
                        compiledOutputMultiple = compiledOutputMultiple.Replace(CodeGeneratorUI.Helpers.AppConfigHelper.GetRightTagUnprocessing(), "}}");
                        compiledOutputMultiple = compiledOutputMultiple.Replace(CodeGeneratorUI.Helpers.AppConfigHelper.GetLeftTagUnprocessing(), "{{");
                        //File.Create(CompiledFileName);
                        File.WriteAllText(compiledFileName, compiledOutputMultiple, System.Text.Encoding.Default);
                    }
                    //end: multiple file
                }
                MdDialog.IsOpen = true;
                Process.Start(outputPath);
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void CopyDirectory(String sourcePath, String destinationPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));            
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        private void OnClickGotoGit(object sender, MouseButtonEventArgs e)
        {
            Process.Start(CodeGeneratorUI.Helpers.AppConfigHelper.GetRepositoryUrl());
        }

        private void OnClickModifyDatabase(object sender, RoutedEventArgs e)
        {
            if (!(ListDatabases.SelectedItem is Button && ((ListDatabases.SelectedItem as Button).Tag is Database))) return;
            Database db = (ListDatabases.SelectedItem as Button).Tag as Database;
            (new WinDatabase(db)).ShowDialog();
            LoadListDatabases();
        }

        private void OnClickDeleteDatabase(object sender, RoutedEventArgs e)
        {
            if (!(((Button)ListDatabases.SelectedItem).Tag is Database)) return;
            _savedDatabases.Delete((((Button)ListDatabases.SelectedItem).Tag as Database).Id);
            LoadListDatabases();
        }

        private void OnClickDeleteTemplate(object sender, RoutedEventArgs e)
        {
            Boolean IsTreeViewItemAndTagDirectoryInfo = TreeTemplates.SelectedItem is TreeViewItem && (TreeTemplates.SelectedItem as TreeViewItem).Tag is DirectoryInfo;
            if (!IsTreeViewItemAndTagDirectoryInfo) return;            
            Template selectedTemplate = _savedTemplates.GetAll().FirstOrDefault(a => a.Path == ((TreeTemplates.SelectedItem as TreeViewItem).Tag as DirectoryInfo).FullName);
            if (selectedTemplate != null)
                _savedTemplates.Delete(selectedTemplate.Id);
            LoadTemplatesTreeView();
        }

        private void treeTemplates_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if (item != null && item.Parent is TreeView)
            {
                item.IsSelected = true;
                ContextMenu ctxMenu = TreeTemplates.Resources["TreeViewTemplateContextMenu"] as ContextMenu;
                if (ctxMenu != null) ctxMenu.IsOpen = true;
            }
        }

        private void OnDoubleClickOpenCodeEditor(object sender, MouseButtonEventArgs e)
        {            
            TreeViewItem selectedTreeViewItem = TreeTemplates.SelectedItem as TreeViewItem;
            Template template = (selectedTreeViewItem.Tag as Template);
            if (!template.IsDirectory)
            new WinCodeEditor(template).ShowDialog();
            LoadTemplatesTreeView();
        }

        private void OnClickOpenPathTemplate(object sender, RoutedEventArgs e)
        {
            Boolean IsTreeViewItemAndTagDirectoryInfo = TreeTemplates.SelectedItem is TreeViewItem && (TreeTemplates.SelectedItem as TreeViewItem).Tag is DirectoryInfo;
            if (!IsTreeViewItemAndTagDirectoryInfo) return;
            String Path = ((TreeTemplates.SelectedItem as TreeViewItem).Tag as Template).Path;
            Process.Start(Path);               
        }
    }
}
