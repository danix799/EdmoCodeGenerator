using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Objects;
using Alphaleonis.Win32.Filesystem;
using Newtonsoft.Json;

namespace CodeGenerator.Helpers
{
    public class DatabaseStorageHelper : StorageHelper
    {
        private string StorageFile = "databases.txt";
        public DatabaseStorageHelper()
        {
            if (!File.Exists(StorageFile))
                File.AppendText(StorageFile, System.Text.Encoding.UTF8);

        }
        public void Save(Database obj)
        {
            List<Database> lines = GetAll();            
            obj.Id = Guid.NewGuid();
            lines.Add(obj);
            WriteJson(StorageFile, lines);            
        }
        public void Delete(Guid Id)
        {
            List<Database> lines = GetAll();
            Database lineToRemove = lines.Where(a => a.Id == Id).First();
            lines.Remove(lineToRemove);
            WriteJson(StorageFile, lines);
        }
        public void Update(Database obj)
        {
            List<Database> lines = GetAll();
            Database lineToUpdate = lines.Where(a => a.Id == obj.Id).First();
            lines.Remove(lineToUpdate);
            lines.Add(obj);
            WriteJson(StorageFile, lines);
        }
        public List<Database> GetAll()
        {
            string json = File.ReadAllText(StorageFile);
            List<Database> lines = JsonConvert.DeserializeObject<List<Database>>(json);
            if (lines == null)
                lines = new List<Database>();
            return lines;
        }

    }
}
