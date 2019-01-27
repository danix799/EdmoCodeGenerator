
using CodeGenerator.Objects;
using DatabaseSchemaReader.DataSchema;
using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
{
    public static class Generator
    {
        public static void Generate(DatabaseSchema Schema, String TemplatePath, String OutputPath)
        {
            IOHelper.DeleteFolderContent(OutputPath);
            IOHelper.CopyDirectory(TemplatePath, OutputPath);
            System.Threading.Thread.Sleep(1);
            String[] FilePaths = Alphaleonis.Win32.Filesystem.Directory.GetFiles(TemplatePath, "*.*",
                SearchOption.AllDirectories);
            foreach(String FilePath in FilePaths)
            {
                Boolean IsMultiGeneration = CheckIsMultiGeneration(FilePath);
                if (IsMultiGeneration)
                    GenerateForMultipleFiles(Schema, FilePath, OutputPath);
                else
                    GenerateForOneFile(Schema, FilePath, OutputPath);
            }
        }
        private static Boolean CheckIsMultiGeneration(String FilePath)
        {
            return DotLiquid.Template.Parse(FilePath).Root.NodeList.Count > 0;
        }
        private static dynamic SchemaToGeneratorSyntax(DatabaseSchema schema)
        {
            return from Table in schema.Tables
                   select new
                   {
                       Name = Table.Name,
                       HasAutoNumberColumn = Table.HasAutoNumberColumn,
                       PrimaryKeyCount = Table.PrimaryKey.Columns.Count,
                       PrimaryKeyFirstName = Table.PrimaryKey.Columns.First(),
                       PrimaryKeyColumns = GetPrimaryKeyColumns(Table),
                       AutoNumberColumns = GetAutoNumberColumns(Table),
                       ForeignKeyColumns = GetForeignKeyColumns(Table),
                       NonAutoNumberColumns = GetNonAutoNumberColumns(Table),
                       NonPrimaryKeyColumns = GetNonPrimaryKeyColumns(Table),
                       NonForeignKeyColumns = GetNonForeignKeyColumns(Table),
                       Columns = GetColumns(Table)
                   };
        }
        private static void GenerateForOneFile(DatabaseSchema Schema, String TemplatePath, String OutputPath)
        {
            var GeneratorSyntax = SchemaToGeneratorSyntax(Schema);
            String fileContents = File.ReadAllText(TemplatePath, System.Text.Encoding.Default);
            DotLiquid.Template templateLiquid = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
            String compiledOutput = templateLiquid.Render(Hash.FromAnonymousObject(new { tables = GeneratorSyntax }, true));
            compiledOutput = compiledOutput.Replace(@"'}'}'", "}}");
            compiledOutput = compiledOutput.Replace(@"'{'{'", "{{");
            File.WriteAllText(OutputPath, compiledOutput);
        }
        private static void GenerateForMultipleFiles(DatabaseSchema Schema, String TemplatePath, String OutputPath)
        {
            String fileContents = File.ReadAllText(TemplatePath, System.Text.Encoding.Default);
            var GeneratorSyntax = SchemaToGeneratorSyntax(Schema);
            foreach (var Table in GeneratorSyntax)
            {
                DotLiquid.Template templateLiquidMultiple = DotLiquid.Template.Parse(@fileContents); // Parses and compiles the template                
                String compiledOutputMultiple = templateLiquidMultiple.Render(Hash.FromAnonymousObject(new { table = Table }, true));
                DotLiquid.Template templateLiquidFileName = DotLiquid.Template.Parse(TemplatePath); // Parses and compiles the template                
                String compiledFileName = templateLiquidFileName.Render(Hash.FromAnonymousObject(new { table = Table }, true));
                compiledOutputMultiple = compiledOutputMultiple.Replace(AppConfigHelper.GetRightTagUnprocessing(), "}}");
                compiledOutputMultiple = compiledOutputMultiple.Replace(AppConfigHelper.GetLeftTagUnprocessing(), "{{");
                File.WriteAllText(compiledFileName, compiledOutputMultiple, System.Text.Encoding.Default);
            }
        }
        private static dynamic GetPrimaryKeyColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   where Column.IsPrimaryKey
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        private static dynamic GetAutoNumberColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   where Column.IsAutoNumber
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        private static dynamic GetForeignKeyColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   where Column.IsForeignKey
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        private static dynamic GetNonAutoNumberColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   where Column.IsAutoNumber
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        private static dynamic GetNonPrimaryKeyColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   where !Column.IsPrimaryKey
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        private static dynamic GetNonForeignKeyColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   where !Column.IsForeignKey
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        private static dynamic GetColumns(DatabaseTable Table)
        {
            return from Column in Table.Columns
                   select new { Name = Column.Name, DataType = Column.DataType.TypeName, Nullable = Column.Nullable, IsPrimaryKey = Column.IsPrimaryKey, IsForeignKey = Column.IsForeignKey, Lenght = Column.Length, IsAutoNumber = Column.IsAutoNumber };
        }
        
    }
}
