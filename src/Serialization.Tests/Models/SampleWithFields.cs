#pragma warning disable SA1401
namespace Serialization.Tests.Models
{
   public class SampleWithFields
   {
      public readonly string ReadonlyField = "can't touch this";
      public int Id;
      public string Name;
      public string MutableField;
   }
}
