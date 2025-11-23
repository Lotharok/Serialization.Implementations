using System;
using System.Collections.Generic;
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

      [Fact]
      public void Serialize_WithCollectionOfComplexTypes_ShouldWork()
      {
         var serializer = new ProtobufSerializer();
         var data = new DataWithCollections
         {
            Name = "Test Collection",
            Items = new List<SampleData>
            {
               new SampleData { Name = "Item1", Age = 10 },
               new SampleData { Name = "Item2", Age = 20 },
            },
            ArrayItems = new[]
            {
               new SampleData { Name = "Array1", Age = 30 },
               new SampleData { Name = "Array2", Age = 40 },
            },
         };

         var bytes = serializer.Serialize(data);
         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();

         var result = serializer.Deserialize<DataWithCollections>(bytes);
         result.Should().NotBeNull();
         result.Name.Should().Be("Test Collection");
         result.Items.Should().HaveCount(2);
         result.ArrayItems.Should().HaveCount(2);
      }

      [Fact]
      public void Serialize_WithCircularReference_ShouldThrowNotSupportedException()
      {
         var serializer = new ProtobufSerializer();
         var data = new CircularReferenceType
         {
            Name = "Parent",
            Child = new CircularReferenceType
            {
               Name = "Child",
            },
         };

         Action act = () => serializer.Serialize(data);
         act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to serialize object*")
            .WithInnerException<NotSupportedException>();
      }

      [Fact]
      public void Deserialize_WithCorruptedBuffer_ShouldThrowInvalidOperationException()
      {
         var serializer = new ProtobufSerializer();
         var corruptedBuffer = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

         Action act = () => serializer.Deserialize<SampleData>(corruptedBuffer);
         act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to deserialize buffer*");
      }

      [Fact]
      public async Task DeserializeAsync_WithCorruptedBuffer_ShouldThrowInvalidOperationException()
      {
         var serializer = new ProtobufSerializer();
         var corruptedBuffer = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

         Func<Task> act = async () => await serializer.DeserializeAsync<SampleData>(corruptedBuffer);
         await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to deserialize buffer*");
      }

      [Fact]
      public void Serialize_WithComplexNestedCollections_ShouldRegisterTypesRecursively()
      {
         var serializer = new ProtobufSerializer();
         var data = new List<DataWithCollections>
         {
            new DataWithCollections
            {
               Name = "Outer1",
               Items = new List<SampleData>
               {
                  new SampleData { Name = "Nested1", Age = 5 },
               },
            },
         };

         var bytes = serializer.Serialize(data);
         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();

         var result = serializer.Deserialize<List<DataWithCollections>>(bytes);
         result.Should().NotBeNull();
         result.Should().HaveCount(1);
      }

      [Fact]
      public void Serialize_WithArrayOfComplexTypes_ShouldHandleArrayTypes()
      {
         var serializer = new ProtobufSerializer();
         var data = new SampleData[]
         {
            new SampleData { Name = "Array1", Age = 15 },
            new SampleData { Name = "Array2", Age = 25 },
         };

         var bytes = serializer.Serialize(data);
         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();

         var result = serializer.Deserialize<SampleData[]>(bytes);
         result.Should().NotBeNull();
         result.Should().HaveCount(2);
         result[0].Name.Should().Be("Array1");
         result[1].Name.Should().Be("Array2");
      }

      [Fact]
      public void EnsureTypeCanBeProcessed_CalledMultipleTimes_ShouldUseCachedResult()
      {
         var serializer = new ProtobufSerializer();

         var data1 = new SampleData { Name = "Test1", Age = 10 };
         var bytes1 = serializer.Serialize(data1);

         var data2 = new SampleData { Name = "Test2", Age = 20 };
         var bytes2 = serializer.Serialize(data2);

         bytes1.Should().NotBeNull();
         bytes2.Should().NotBeNull();
      }
   }
}
