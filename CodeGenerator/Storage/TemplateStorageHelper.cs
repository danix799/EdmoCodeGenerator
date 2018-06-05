using System;
using System.Collections.Generic;
using System.Linq;
using Alphaleonis.Win32.Filesystem;
using CodeGenerator.Objects;
using Newtonsoft.Json;

namespace CodeGenerator.Storage
{
    public class TemplateStorageHelper : StorageHelper
    {
        private string StorageFile = "templates.txt";
        public TemplateStorageHelper()
        {
            if (!File.Exists(StorageFile))
                File.Create(StorageFile);
        }
        public void Save(Template obj)
        {
            List<Template> lines = GetAll();
            obj.Id = Guid.NewGuid();
            lines.Add(obj);
            WriteJson(StorageFile, lines);
        }
        public void Delete(Guid id)
        {
            List<Template> lines = GetAll();
            Template lineToRemove = lines.First(a => a.Id == id);
            lines.Remove(lineToRemove);
            WriteJson(StorageFile, lines);
        }

        public List<Template> GetAll()
        {
            string json = File.ReadAllText(StorageFile);
            List<Template> lines = JsonConvert.DeserializeObject<List<Template>>(json) ?? new List<Template>();
            return lines;
        }
    }
}
