using System;
using System.Threading.Tasks;
using FluentAssertions;
using Serialization.Tests.Models;
using Serialization.Vendor.Newtonsoft;
using Xunit;

namespace Serialization.Tests
{
   public class JsonStringSerializerTests
   {
      private readonly JsonStringSerializer serializer = new JsonStringSerializer();

      [Fact]
      public void Serialize_ShouldSerializeObjectToJson()
      {
         var data = new SampleData { Age = 30, Name = "Alice", DateOfBirth = new DateTime(2020, 01, 01) };
         var json = this.serializer.Serialize(data);

         json.Should().Contain("\"Name\":\"Alice\"");
         json.Should().Contain("\"Age\":30");
      }

      [Fact]
      public void Deserialize_ShouldReturnExpectedObject()
      {
         var json = "{\"Name\":\"Bob\",\"Age\":25, \"DateOfBirth\":\"2022-12-02\"}";
         var result = this.serializer.Deserialize<SampleData>(json);

         result.Should().NotBeNull();
         result.Name.Should().Be("Bob");
         result.Age.Should().Be(25);
         result.DateOfBirth.Should().Be(new DateTime(2022, 12, 02));
      }

      [Fact]
      public void Constructor_WithNullSettings_ShouldThrow()
      {
         Action act = () => _ = new JsonStringSerializer(null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Deserialize_NullInput_ShouldThrow()
      {
         Action act = () => this.serializer.Deserialize<SampleData>(null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Deserialize_EmptyString_ShouldThrow()
      {
         Action act = () => this.serializer.Deserialize<SampleData>(" ");
         act.Should().Throw<ArgumentException>()
             .WithMessage("Buffer cannot be empty or whitespace.*");
      }

      [Fact]
      public void Serialize_InvalidObject_ShouldThrow()
      {
         var obj = new { Name = "Test", Throw = new ThrowingProperty() };

         Action act = () => this.serializer.Serialize(obj);
         act.Should().Throw<InvalidOperationException>()
             .WithMessage("Unexpected error during serialization of type *ThrowingProperty*");
      }

      [Fact]
      public void Deserialize_InvalidJson_ShouldThrow()
      {
         string invalidJson = "{ invalid json }";
         Action act = () => this.serializer.Deserialize<SampleData>(invalidJson);
         act.Should().Throw<InvalidOperationException>()
             .WithMessage("Unexpected error during deserialization to type *");
      }

      [Fact]
      public async Task SerializeAsync_ShouldWorkCorrectly()
      {
         var data = new SampleData { Age = 40, Name = "AsyncUser", DateOfBirth = new DateTime(2020, 01, 01) };
         var json = await this.serializer.SerializeAsync(data);

         json.Should().Contain("\"AsyncUser\"");
      }

      [Fact]
      public async Task DeserializeAsync_ShouldWorkCorrectly()
      {
         var json = "{\"Name\":\"AsyncBob\",\"Age\":22}";
         var result = await this.serializer.DeserializeAsync<SampleData>(json);

         result.Name.Should().Be("AsyncBob");
         result.Age.Should().Be(22);
      }

      private class ThrowingProperty
      {
         public string Value => throw new Exception("Cannot access property");
      }
   }
}
