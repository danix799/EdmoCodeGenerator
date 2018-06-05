using System;
using Alphaleonis.Win32.Filesystem;
namespace CodeGenerator.Objects
{
   public class Template
    {
        public Guid Id { set; get; }

        public string Path { private set; get; }

        public Boolean IsDirectory { private set; get; }

        public Boolean MultipleFiles { set; get; }

        public Template(String path) {
            Path = path;
            MultipleFiles = true;
            IsDirectory = CheckIsDirectory(path);
        
        }
       private Boolean CheckIsDirectory(String path)
        {
            var attr = File.GetAttributes(path);
            return (attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }

    }
}
