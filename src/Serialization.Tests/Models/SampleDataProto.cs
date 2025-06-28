using System;
using ProtoBuf;

namespace Serialization.Tests.Models
{
   [ProtoContract]
   public class SampleDataProto
   {
      [ProtoMember(1)]
      public string Name { get; set; }

      [ProtoMember(2)]
      public int Age { get; set; }

      [ProtoMember(3)]
      public DateTime DateOfBirth { get; set; }
   }
}
