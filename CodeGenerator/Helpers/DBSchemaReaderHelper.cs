using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using CodeGenerator.Objects;
namespace CodeGenerator.Helpers
{
   public class DbSchemaReaderHelper
    {

       public DatabaseSchema GetSchema(Database db)
       {
           //https://github.com/martinjw/dbschemareader/wiki/Schema-Reading
           var dbReader = new DatabaseReader(db.ConnectionString,db.Provider);
           dbReader.Owner = db.Owner ;
           return dbReader.ReadAll();           
       }
       
    }
}
