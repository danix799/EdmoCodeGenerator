using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
namespace CodeGenerator.Objects
{
   public class Template
    {
        public Guid Id { set; get; }

        public string Path { set; get; }

        public Boolean IsDirectory { set; get; }

        public Boolean MultipleFiles { set; get; }

        public Template(String Path) {
            this.Path = Path;
            this.MultipleFiles = true;
            this.IsDirectory = CheckIsDirectory(Path);
        
        }
        public IEnumerable<FileIdBothDirectoryInfo> GetDirectoriesAndFiles() {
            Boolean IsDirectory = this.CheckIsDirectory(Path);
            this.Path = Path;
            if (IsDirectory)
                return Directory.EnumerateFileIdBothDirectoryInfo(Path, PathFormat.FullPath);
            else
                return null;
        }
        private Boolean CheckIsDirectory(String Path)
        {
            var attr = Alphaleonis.Win32.Filesystem.File.GetAttributes(Path);
            return (attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }

    }
}
