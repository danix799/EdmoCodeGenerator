using System;

namespace CodeGenerator.Objects
{
  public  class Database
    {
      public Guid Id { set; get; }
      public string Name { set; get; }
      public String Provider { set; get; }
      public string ConnectionString { set; get; }
      public string Owner { set; get; }
    }
}
