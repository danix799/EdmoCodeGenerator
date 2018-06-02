using Alphaleonis.Win32.Filesystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CodeGenerator.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
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
        public void Delete(Guid Id)
        {
            List<Template> lines = GetAll();
            Template lineToRemove = lines.Where(a => a.Id == Id).First();
            lines.Remove(lineToRemove);
            WriteJson(StorageFile, lines);
        }
        public void Update(Template obj)
        {
            List<Template> lines = GetAll();
            Template lineToUpdate = lines.Where(a => a.Id == obj.Id).First();
            lines.Remove(lineToUpdate);
            lines.Add(lineToUpdate);
            WriteJson(StorageFile, lines);
        }
        public List<Template> GetAll()
        {
            string json = File.ReadAllText(StorageFile);
            List<Template> lines = JsonConvert.DeserializeObject<List<Template>>(json);
            if (lines == null)
                lines = new List<Template>();
            return lines;
        }
    }
}
