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

        public MainWindow()
        {
            InitializeComponent();
            LoadListDatabases();
            LoadTemplatesTreeView();
        }

        private void LoadListDatabases()
        {
            ListDatabases.Items.Clear();
            List<Database> databases = _savedDatabases.GetAll();
            if (databases.Count == 0)
            {
                GridSectionDatabase.Children.Add(new MessageNoDatabase());
                return;
            }
            MessageNoDatabase messageNoDatabase = GridSectionDatabase.Children.OfType<MessageNoDatabase>().FirstOrDefault();
            GridSectionDatabase.Children.Remove(messageNoDatabase);

            foreach (Database db in databases)
            {
                Button btn = new Button {Style = Application.Current.Resources["MaterialDesignToolButton"] as Style};

                PackIcon icon = new PackIcon
                {
                    Kind = PackIconKind.Database,
                    Style = Application.Current.Resources["MaterialDesignButtonInlineIcon"] as Style
                };

                TextBlock text = new TextBlock {Text = db.Name};

                StackPanel sp = new StackPanel {Orientation = Orientation.Horizontal};

                sp.Children.Add(icon);
                sp.Children.Add(text);
                btn.Content = sp;

                btn.Tag = db;

                ListDatabases.Items.Add(btn);
            }
        }

        private void OnClickCreateDatabase(object sender, RoutedEventArgs e)
        {
            new WinDatabase().ShowDialog();
            LoadListDatabases();
        }

        private void OnClickListDatabase(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            if (btn == null) return;
            Database db = btn.Tag as Database;
            DatabaseSchema schema = _schemaReader.GetSchema(db);

            TreeSchema.Items.Clear();
            TreeViewItem tviTables = new TreeViewItem
            {
                Header = "Tables",
                Tag = schema.Tables
            };
            TreeSchema.Items.Add(tviTables);
            foreach (DatabaseTable dbTable in schema.Tables)
            {
                tviTables.Items.Add(LoadTablesFromSchema(dbTable));
            }
        }

        private TreeViewItem LoadTablesFromSchema(DatabaseTable dbTable)
        {
            TreeViewItem tvi = new TreeViewItem
            {
                Header = dbTable.Name,
                Tag = dbTable
            };
            foreach (DatabaseColumn dbColumn in dbTable.Columns)
            {
                tvi.Items.Add(LoadColumnsFromTables(dbColumn));
            }
            return tvi;
        }
        private TreeViewItem LoadColumnsFromTables(DatabaseColumn dbColumn)
        {
            TreeViewItem tvi = new TreeViewItem
            {
                Header = dbColumn.Name,
                Tag = dbColumn
            };
            return tvi;
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

            if (templates.Count == 0)
            {
                GridSectionTemplates.Children.Add(new MessageNoTemplate());
                return;
            }
            MessageNoTemplate messageNoTemplate = GridSectionTemplates.Children.OfType<MessageNoTemplate>().FirstOrDefault();
            GridSectionTemplates.Children.Remove(messageNoTemplate);

            foreach (Template obj in templates)
            {
                if (obj.IsDirectory)
                {
                    var rootDirectoryInfo = new DirectoryInfo(obj.Path);
                    TreeTemplates.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
                }

            }
        }

        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name, Tag = directoryInfo };
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Items.Add(CreateDirectoryNode(directory));

            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Items.Add(new TreeViewItem { Header = file.Name });

            return directoryNode;

        }

        private void DeleteFolderContent(String Path)
        {
            DirectoryInfo di = new DirectoryInfo(Path);

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
                Button button = ListDatabases.Items[0] as Button;
                if (button == null) return;
                Database db = (button.Tag as Database);
                Template template = _savedTemplates.GetAll().First();
                DatabaseSchema schema = _schemaReader.GetSchema(db);
                String outputPath = @"C:\Users\user\Desktop\output"; //TODO: generate for all output paths

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
                                   Columns = from y in x.Columns
                                             select new { Name = y.Name, DataType = y.DataType.TypeName, Nullable = y.Nullable, IsPrimaryKey = y.IsPrimaryKey, IsForeignKey = y.IsForeignKey, Lenght = y.Length, IsAutoNumber = y.IsAutoNumber }
                               };

                    

                    
                    // start: single file
                    DotLiquid.Template templateLiquid = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                    String compiledOutput = templateLiquid.Render(Hash.FromAnonymousObject(new { tables = anom }, true));
                    File.WriteAllText(path, compiledOutput);
                    // end: single file

                    //start: multiple file
                    foreach (var table in anom)
                    {
                        DotLiquid.Template templateLiquidMultiple = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                        String compiledOutputMultiple = templateLiquidMultiple.Render(Hash.FromAnonymousObject(new { table = table }, true));
                        DotLiquid.Template templateLiquidFileName = DotLiquid.Template.Parse(path); // Parses and compiles the template                
                        String compiledFileName = templateLiquidFileName.Render(Hash.FromAnonymousObject(new { table = table }, true));
                        compiledOutputMultiple = compiledOutputMultiple.Replace(@"'}'}'", "}}");
                        compiledOutputMultiple = compiledOutputMultiple.Replace(@"'{'{'", "{{");
                        //File.Create(CompiledFileName);
                        File.WriteAllText(compiledFileName, compiledOutputMultiple, System.Text.Encoding.Default);
                    }
                    //end: multiple file
                }
                MdDialog.IsOpen = true;
                Process.Start(outputPath);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void CopyDirectory(String sourcePath, String destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        private void OnClickGotoGit(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/danix799/EdmoCodeGenerator");
        }

        private void OnClickModifyDatabase(object sender, RoutedEventArgs e)
        {
            var button = ListDatabases.SelectedItem as Button;
            if (button != null)
            {
                Database db = button.Tag as Database;
                if (db != null)
                {
                    (new WinDatabase(db)).ShowDialog();
                    LoadListDatabases();
                }
            }
        }

        private void OnClickDeleteDatabase(object sender, RoutedEventArgs e)
        {
            Database db = ((Button) ListDatabases.SelectedItem).Tag as Database;
            if (db != null)
            {
                _savedDatabases.Delete(db.Id);
                LoadListDatabases();
            }
        }

        private void OnClickDeleteTemplate(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = TreeTemplates.SelectedItem as TreeViewItem;
            if (selectedTreeViewItem != null)
            {
                DirectoryInfo directoryInfo = selectedTreeViewItem.Tag as DirectoryInfo;
                if (directoryInfo != null)
                {
                    String path = directoryInfo.FullName; //TODO: CHECK IF IS DIRECTORY INFO OR FILEINFO OR TEMPLATE
                    Template selectedTemplate = _savedTemplates.GetAll().FirstOrDefault(a => a.Path == path);
                    if (selectedTemplate != null)
                    {
                        _savedTemplates.Delete(selectedTemplate.Id);
                        LoadTemplatesTreeView();
                    }
                }
            }
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
            (new WinCodeEditor()).ShowDialog();
        }

        private void OnClickOpenPathTemplate(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = TreeTemplates.SelectedItem as TreeViewItem;
            if (selectedTreeViewItem != null)
            {
                var directoryInfo = selectedTreeViewItem.Tag as DirectoryInfo;
                if (directoryInfo != null)
                {
                    Process.Start(directoryInfo.FullName);
                }
            }
        } 
       
    }
}
