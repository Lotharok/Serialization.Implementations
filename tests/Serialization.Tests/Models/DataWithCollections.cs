using System.Collections.Generic;

namespace Serialization.Tests.Models
{
   public class DataWithCollections
   {
      public string Name { get; set; }

      public List<SampleData> Items { get; set; }

      public SampleData[] ArrayItems { get; set; }
   }
}
