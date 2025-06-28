using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ProtoBuf.Meta;
using Serialization.Tests.Models;
using Serialization.Vendor.Protobuf;
using Xunit;

namespace Serialization.Tests
{
   public class ProtobufSerializerTests
   {
      private readonly ProtobufSerializer serializer = new ProtobufSerializer();

      [Fact]
      public void Serialize_ShouldReturnByteArray()
      {
         var data = new SampleDataProto
         {
            Name = "Alice",
            Age = 30,
            DateOfBirth = DateTime.Now,
         };
         var bytes = this.serializer.Serialize(data);

         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();
      }

      [Fact]
      public void Deserialize_ShouldReturnExpectedObject()
      {
         var data = new SampleDataProto
         {
            Name = "Bob",
            Age = 25,
            DateOfBirth = DateTime.Now,
         };
         var bytes = this.serializer.Serialize(data);

         var result = this.serializer.Deserialize<SampleDataProto>(bytes);

         result.Should().NotBeNull();
         result.Name.Should().Be("Bob");
         result.Age.Should().Be(25);
      }

      [Fact]
      public void Constructor_WithNullModel_ShouldThrow()
      {
         Action act = () => _ = new ProtobufSerializer(null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Serialize_NullValue_ShouldThrow()
      {
         Action act = () => this.serializer.Serialize<SampleDataProto>(null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Deserialize_NullBuffer_ShouldThrow()
      {
         Action act = () => this.serializer.Deserialize<SampleDataProto>(null);
         act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Deserialize_EmptyBuffer_ShouldThrow()
      {
         Action act = () => this.serializer.Deserialize<SampleDataProto>(Array.Empty<byte>());
         act.Should().Throw<ArgumentException>().WithMessage("Buffer cannot be empty.*");
      }

      [Fact]
      public void Serialize_NoContract_ShouldReturnByteArray()
      {
         var data = new SampleData
         {
            Name = "Alice",
            Age = 30,
            DateOfBirth = DateTime.Now,
         };
         var bytes = this.serializer.Serialize(data);

         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();
      }

      [Fact]
      public void Deserialize_NoContract_ShouldReturnExpectedObject()
      {
         var data = new SampleData
         {
            Name = "Bob",
            Age = 25,
            DateOfBirth = DateTime.Now,
         };
         var bytes = this.serializer.Serialize(data);

         var result = this.serializer.Deserialize<SampleData>(bytes);

         result.Should().NotBeNull();
         result.Name.Should().Be("Bob");
         result.Age.Should().Be(25);
      }

      [Fact]
      public async Task SerializeAsync_ShouldWorkCorrectly()
      {
         var data = new SampleDataProto
         {
            Name = "AsyncAlice",
            Age = 40,
            DateOfBirth = DateTime.Now,
         };
         var bytes = await this.serializer.SerializeAsync(data);

         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();
      }

      [Fact]
      public async Task DeserializeAsync_ShouldWorkCorrectly()
      {
         var data = new SampleDataProto
         {
            Name = "AsyncBob",
            Age = 22,
            DateOfBirth = DateTime.Now,
         };
         var bytes = await this.serializer.SerializeAsync(data);

         var result = await this.serializer.DeserializeAsync<SampleDataProto>(bytes);

         result.Name.Should().Be("AsyncBob");
         result.Age.Should().Be(22);
      }

      [Fact]
      public void Serialize_ShouldRegisterOnlyNonReadonlyFields()
      {
         var serializer = new ProtobufSerializer();
         var data = new SampleWithFields
         {
            Id = 1,
            Name = "Field Test",
            MutableField = "This is mutable",
         };

         var bytes = serializer.Serialize(data);

         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();

         var result = serializer.Deserialize<SampleWithFields>(bytes);
         result.Should().NotBeNull();
         result.Id.Should().Be(1);
         result.Name.Should().Be("Field Test");
         result.MutableField.Should().Be("This is mutable");
      }

      [Fact]
      public void Serialize_ShouldIncludeOnlyValidProperties()
      {
         var serializer = new ProtobufSerializer();
         var obj = new PropertyEdgeCases
         {
            ValidProperty = "included",
         };

         var bytes = serializer.Serialize(obj);
         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();

         var result = serializer.Deserialize<PropertyEdgeCases>(bytes);
         result.Should().NotBeNull();
         result.ValidProperty.Should().Be("included");

         result.ReadOnlyProperty.Should().Be("readonly");
      }
   }
}
