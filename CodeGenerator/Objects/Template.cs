using System;
using Alphaleonis.Win32.Filesystem;
using System.Collections.Generic;

namespace CodeGenerator.Objects
{
    public class Template
    {
        public String Name { get { return this._ComputeName(); } }
        public Guid Id { set; get; }

        public string Path { private set; get; }

        public Boolean IsDirectory { private set; get; }

        public Boolean MultipleFiles { set; get; }

        public Boolean ExistsInFileSystem { set; get; }

        public Object FileSystemInfo { get { return _ComputeFileSystemInfo(); } }

        public Template(String path)
        {
            ExistsInFileSystem = Directory.Exists(path) || File.Exists(path);
            Path = path;
            MultipleFiles = true;
            if (ExistsInFileSystem)
                IsDirectory = CheckIsDirectory(path);

        }
        public static List<Template> PathsToTemplates(String[] Paths)
        {
            List<Template> templates = new List<Template>();
            foreach (string path in Paths)
                templates.Add(new Template(path));
            return templates;
        }
        public static List<Template> PathsToTemplates(DirectoryInfo[] DirectoryInfos)
        {
            List<Template> templates = new List<Template>();
            foreach (DirectoryInfo path in DirectoryInfos)
                templates.Add(new Template(path.FullName));
            return templates;
        }
        public static List<Template> PathsToTemplates(FileInfo[] FileInfos)
        {
            List<Template> templates = new List<Template>();
            foreach (FileInfo path in FileInfos)
                templates.Add(new Template(path.FullName));
            return templates;
        }

        public List<Template> GetChilds()
        {
            List<Template> childs = new List<Template>();
            if (this.IsDirectory){
                childs.AddRange(PathsToTemplates((this.FileSystemInfo as DirectoryInfo).GetDirectories()));
                childs.AddRange(PathsToTemplates((this.FileSystemInfo as DirectoryInfo).GetFiles()));
            }            
            return childs;
        }

        private Object _ComputeFileSystemInfo()
        {
            if (ExistsInFileSystem)
            {
                if (IsDirectory)
                    return new DirectoryInfo(this.Path);
                return new FileInfo(this.Path);
            }
            return null;
        }

        private string _ComputeName()
        {
            if (!ExistsInFileSystem)
                return this.Path + " (Not Found)";
            return this.Path;
        }


        private Boolean CheckIsDirectory(String path)
        {
            var attr = File.GetAttributes(path);
            return (attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }

    }
}
