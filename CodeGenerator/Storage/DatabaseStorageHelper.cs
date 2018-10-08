using System;
using System.Collections.Generic;
using System.Linq;
using Alphaleonis.Win32.Filesystem;
using CodeGenerator.Objects;
using Newtonsoft.Json;
using CodeGenerator.Helpers;

namespace CodeGenerator.Storage
{
    public class DatabaseStorageHelper : StorageHelper
    {
        private string StorageFile { get { return AppConfigHelper.GetDatabaseStorageFile(); } }
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
        public void Delete(Guid id)
        {
            List<Database> lines = GetAll();
            Database lineToRemove = lines.First(a => a.Id == id);
            lines.Remove(lineToRemove);
            WriteJson(StorageFile, lines);
        }
        public void Update(Database obj)
        {
            List<Database> lines = GetAll();
            Database lineToUpdate = lines.First(a => a.Id == obj.Id);
            lines.Remove(lineToUpdate);
            lines.Add(obj);
            WriteJson(StorageFile, lines);
        }
        public List<Database> GetAll()
        {
            string json = File.ReadAllText(StorageFile);
            List<Database> lines = JsonConvert.DeserializeObject<List<Database>>(json) ?? new List<Database>();
            return lines;
        }

    }
}
