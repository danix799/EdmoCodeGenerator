using Alphaleonis.Win32.Filesystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
{
  public  class StorageHelper
  {
      public void WriteJson(String StorageFile, Object lines) {
          string json = JsonConvert.SerializeObject(lines);
          File.WriteAllText(StorageFile, json);
      }
    }
}
