using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
{
    public static class IOHelper
    {
        public static void DeleteFolderContent(String Path)
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
        public static void CopyDirectory(String sourcePath, String destinationPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                System.IO.SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                System.IO.SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }
        public static bool IsFileLocked(FileInfo file)
        {
            System.IO.FileStream stream = null;
            try
            {
                //try to get a file lock
                stream = file.Open(System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
            }
            catch (System.IO.IOException)
            {
                //File isn't ready yet, so return true as it is still looked --> we need to keep on waiting
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            // At the end, when stream is closed and disposed and no exception occured, return false --> File is not locked anymore
            return false;
        }
        public static bool IsDirectoryLocked(String Path)
        {
            System.IO.FileStream stream = null;
            try
            {
                //try to get a file lock
                DirectoryInfo directory = new DirectoryInfo(Path);
                var x = directory.GetFileSystemInfos();
            }
            catch (System.IO.IOException)
            {
                //File isn't ready yet, so return true as it is still looked --> we need to keep on waiting
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            // At the end, when stream is closed and disposed and no exception occured, return false --> File is not locked anymore
            return false;
        }
    }
}
