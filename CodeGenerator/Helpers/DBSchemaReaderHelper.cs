using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using CodeGenerator.Objects;
namespace CodeGenerator.Helpers
{
   public class DBSchemaReaderHelper
    {

       public DatabaseSchema GetSchema(Database db)
       {
           //https://github.com/martinjw/dbschemareader/wiki/Schema-Reading
           var dbReader = new DatabaseReader(db.ConnectionString,db.Provider);

           dbReader.Owner = "SALARYWORKSHEET";
           return dbReader.ReadAll();           
       }
       
    }
}
