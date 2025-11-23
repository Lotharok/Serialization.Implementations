namespace Serialization.Tests.Models
{
   public class CircularReferenceType
   {
      public string Name { get; set; }

      public CircularReferenceType Child { get; set; }
   }
}
