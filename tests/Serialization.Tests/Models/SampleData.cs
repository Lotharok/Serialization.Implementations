using System;
using MessagePack;

namespace Serialization.Tests.Models
{
   [MessagePackObject]
   public class SampleData
   {
      [Key(0)]
      public string Name { get; set; }

      [Key(1)]
      public int Age { get; set; }

      [Key(2)]
      public DateTime DateOfBirth { get; set; }
   }
}
