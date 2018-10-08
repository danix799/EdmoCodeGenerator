using System;
using Alphaleonis.Win32.Filesystem;
using Newtonsoft.Json;

namespace CodeGenerator.Storage
{
  public  class StorageHelper
  {
      protected void WriteJson(String storageFile, Object lines) {
          string json = JsonConvert.SerializeObject(lines, Formatting.Indented, new JsonSerializerSettings
          {
              ReferenceLoopHandling = ReferenceLoopHandling.Ignore
          });
          File.WriteAllText(storageFile, json);
      }
    }
}
