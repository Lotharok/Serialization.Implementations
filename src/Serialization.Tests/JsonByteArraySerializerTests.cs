using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Serialization.Tests.Models;
using Serialization.Vendor.Newtonsoft;
using Xunit;

namespace Serialization.Tests
{
   public class JsonByteArraySerializerTests
   {
      private readonly JsonByteArraySerializer serializer = new JsonByteArraySerializer();

      [Fact]
      public void Serialize_ShouldReturnByteArray()
      {
         var data = new SampleData { Age = 30, Name = "Alice", DateOfBirth = new DateTime(2020, 01, 01) };
         var bytes = this.serializer.Serialize(data);

         bytes.Should().NotBeNull();
         var json = Encoding.UTF8.GetString(bytes);
         json.Should().Contain("\"Alice\"");
      }

      [Fact]
      public void Deserialize_ShouldReturnExpectedObject()
      {
         var json = "{\"Name\":\"Bob\",\"Age\":25, \"DateOfBirth\":\"2022-12-02\"}";
         var bytes = Encoding.UTF8.GetBytes(json);
         var result = this.serializer.Deserialize<SampleData>(bytes);

         result.Name.Should().Be("Bob");
         result.Age.Should().Be(25);
         result.DateOfBirth.Should().Be(new DateTime(2022, 12, 02));
      }

      [Fact]
      public void Constructor_WithNullSettings_ShouldThrow()
      {
         Action act = () => _ = new JsonByteArraySerializer(null, Encoding.UTF8);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Constructor_WithNullEncoding_ShouldThrow()
      {
         Action act = () => _ = new JsonByteArraySerializer(new JsonSerializerSettings(), null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Constructor_Correctly()
      {
         Action act = () => _ = new JsonByteArraySerializer(new JsonSerializerSettings());
         act.Should().NotThrow<ArgumentNullException>();
      }

      [Fact]
      public void Deserialize_NullBuffer_ShouldThrow()
      {
         Action act = () => this.serializer.Deserialize<SampleData>(null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Deserialize_EmptyBuffer_ShouldThrow()
      {
         Action act = () => this.serializer.Deserialize<SampleData>(Array.Empty<byte>());
         act.Should().Throw<ArgumentException>().WithMessage("Buffer cannot be empty.*");
      }

      [Fact]
      public void Deserialize_InvalidJson_ShouldThrow()
      {
         var invalidBytes = Encoding.UTF8.GetBytes("{ invalid json }");
         Action act = () => this.serializer.Deserialize<SampleData>(invalidBytes);
         act.Should().Throw<InvalidOperationException>()
             .WithMessage("Unexpected error during deserialization to type*");
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
      public async Task SerializeAsync_ShouldReturnByteArray()
      {
         var data = new SampleData { Age = 40, Name = "AsyncUser", DateOfBirth = new DateTime(2020, 01, 01) };
         var bytes = await this.serializer.SerializeAsync(data);

         bytes.Should().NotBeNull();
         Encoding.UTF8.GetString(bytes).Should().Contain("\"AsyncUser\"");
      }

      [Fact]
      public async Task DeserializeAsync_ShouldReturnExpectedObject()
      {
         var json = "{\"Name\":\"AsyncBob\",\"Age\":22}";
         var bytes = Encoding.UTF8.GetBytes(json);
         var result = await this.serializer.DeserializeAsync<SampleData>(bytes);

         result.Name.Should().Be("AsyncBob");
         result.Age.Should().Be(22);
      }

      private class ThrowingProperty
      {
         public string Value => throw new Exception("Cannot access property");
      }
   }
}
