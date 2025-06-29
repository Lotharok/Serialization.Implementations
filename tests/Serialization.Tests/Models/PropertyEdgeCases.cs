namespace Serialization.Tests.Models
{
   public class PropertyEdgeCases
   {
      public string ValidProperty { get; set; }

      public string ReadOnlyProperty => "readonly";

      public string WriteOnlyProperty { private get; set; }

      public string this[int index] => "indexer";
   }
}
