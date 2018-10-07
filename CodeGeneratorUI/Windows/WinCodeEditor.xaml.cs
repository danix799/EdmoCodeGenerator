
using CodeGenerator.Objects;
using Alphaleonis.Win32.Filesystem;
using System.Diagnostics;
using System.Windows;
using Microsoft.VisualBasic.FileIO;

namespace CodeGeneratorUI.Windows
{
    /// <summary>
    /// Lógica de interacción para WinCodeEditor.xaml
    /// </summary>
    public partial class WinCodeEditor
    {
        Template SourceTemplate;
        public WinCodeEditor(Template SourceTemplate)
        {
            InitializeComponent();
            this.SourceTemplate = SourceTemplate;
            FileInfo FileInformation = (SourceTemplate.FileSystemInfo as FileInfo);
            this.textEditor.Text = FileInformation.OpenText().ReadToEnd();
            this.textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(FileInformation.Extension);
        }

        private void OnClickSee(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(SourceTemplate.Path);
        }

        private void OnClickSave(object sender, System.Windows.RoutedEventArgs e)
        {
            File.WriteAllText(SourceTemplate.Path, this.textEditor.Text);
            MessageBox.Show(Properties.Resources.file_saved_successfull);
            this.Close();
        }

        private void OnClickDelete(object sender, System.Windows.RoutedEventArgs e)
        {
            FileSystem.DeleteFile(SourceTemplate.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            MessageBox.Show(Properties.Resources.file_deleted_successfull);
            this.Close();
        }
    }
}
