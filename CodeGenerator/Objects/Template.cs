using System;
using Alphaleonis.Win32.Filesystem;
using System.Collections.Generic;


namespace CodeGenerator.Objects
{
    public class Template
    {
        public event System.IO.FileSystemEventHandler Changed;
        public event System.IO.FileSystemEventHandler Created;
        public event System.IO.FileSystemEventHandler Deleted;
        public event System.IO.RenamedEventHandler Renamed;

        public System.IO.FileSystemWatcher watcher;
        public String Name { get { return this._ComputeName(); } }
        public Guid Id { set; get; }

        public string Path { private set; get; }

        public Boolean IsDirectory { private set; get; }

        public Boolean MultipleFiles { set; get; }

        public Boolean ExistsInFileSystem { set; get; }

        public Object FileSystemInfo { get { return _ComputeFileSystemInfo(); } }

        public Boolean IsBaseTemplate { set; get; }

        public Template(String path)
        {
            ExistsInFileSystem = Directory.Exists(path) || File.Exists(path);
            Path = path;
            MultipleFiles = true;
            if (ExistsInFileSystem)
            {
                IsDirectory = CheckIsDirectory(path);
                InitFileSystemEvents();
            }
                

        }
        public void InitFileSystemEvents()
        {
            if (this.IsBaseTemplate) {
                watcher = new System.IO.FileSystemWatcher();
                watcher.Path = this.Path;
                watcher.NotifyFilter = System.IO.NotifyFilters.LastAccess | System.IO.NotifyFilters.LastWrite
                   | System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.DirectoryName;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
            }

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
            String Result = "";
            if (!ExistsInFileSystem)
            {
                Result =  "Not found file";
            }
            if (_ComputeFileSystemInfo() is DirectoryInfo)
                Result = (_ComputeFileSystemInfo() as DirectoryInfo).Name;
            if (_ComputeFileSystemInfo() is FileInfo)
                Result =(_ComputeFileSystemInfo() as FileInfo).Name;
            return Result;
        }


        private Boolean CheckIsDirectory(String path)
        {
            var attr = File.GetAttributes(path);
            return (attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }

    }
}
