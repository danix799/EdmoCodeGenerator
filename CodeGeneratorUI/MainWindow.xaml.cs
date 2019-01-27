using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MaterialDesignThemes.Wpf;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using Template = CodeGenerator.Objects.Template;
using TreeView = System.Windows.Controls.TreeView;
using CodeGeneratorUI.Helpers;
using CodeGeneratorUI.Validators;
using System.IO;
using System.Threading;

namespace CodeGeneratorUI
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        String OutputPath = @"C:\Users\user-pc\Desktop\output";
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
                LoadtLastSelectedDatabase();
                LoadTemplatesTreeView();
            }
            catch (ValidationException exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void LoadtLastSelectedDatabase()
        {
            if (Helpers.AppConfigHelper.ExistsLastSelectedDatabase())
            {
                Guid id = Helpers.AppConfigHelper.GetLastSelectedDatabase();
                Database LastSelectedDatabase = _savedDatabases.GetAll().First(db => db.Id == id);
                Button LastSelectedDatabaseButtonInTree =  ListDatabases.Items.Cast<Button>().First(btn => (btn.Tag as Database).Id == id);
                ListDatabases.SelectedItem = LastSelectedDatabaseButtonInTree;
                TreeSchema.Items.Clear();
                DatabaseSchema schema = _schemaReader.GetSchema(LastSelectedDatabase);
                this.LoadTablesFromDatabase(schema);

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
            if (!(e.Source is Button)) return;
            Database db = (e.Source as Button).Tag as Database;            
            DatabaseSchema schema = _schemaReader.GetSchema(db);
            Helpers.AppConfigHelper.SetLastSelectedDatabase(db.Id);
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
                obj.IsBaseTemplate = true;
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
            {
                TreeTemplates.Items.Add(CreateTemplateNode(obj));
                obj.InitFileSystemEvents();
                obj.watcher.Deleted += OnTemplateCreatedDeletedRenamed;
                obj.watcher.Created += OnTemplateCreatedDeletedRenamed;
                obj.watcher.Renamed += OnTemplateCreatedDeletedRenamed;
            }
            ToogleMessageNoTemplates();
        }

        private void OnTemplateCreatedDeletedRenamed(object sender, FileSystemEventArgs e)
        {
            //while (IOHelper.IsFileLocked(fInfo))
            //{
            //    Console.WriteLine("File not ready to use yet (copy process ongoing)");
            //    Thread.Sleep(5);  //Wait for 5ms
            //}
            LoadTemplatesTreeView();
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

        

        private void OnClickGenerate(object sender, RoutedEventArgs e)
        {
            try
            {
                (new MainWindowValidator(this)).ValidateGenerate();
                Button button = ListDatabases.SelectedItem as Button;
                if (button == null) return;
                Database db = (button.Tag as Database);
                Template template = _savedTemplates.GetAll().First();
                DatabaseSchema schema = _schemaReader.GetSchema(db);
                Generator.Generate(schema, template.Path, OutputPath);
                MdDialog.IsOpen = true;
                Process.Start(OutputPath);
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message);
            }

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
            if (!template.IsDirectory) new WinCodeEditor(template).ShowDialog();            
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
