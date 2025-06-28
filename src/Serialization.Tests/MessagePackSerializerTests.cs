using System;
using System.Threading.Tasks;
using FluentAssertions;
using MessagePack;
using MessagePack.Resolvers;
using Serialization.Tests.Models;
using Serialization.Vendor.MessagePack;
using Xunit;

namespace Serialization.Tests
{
   public class MessagePackSerializerTests
   {
      private readonly MessageSerializer serializer = new MessageSerializer();

      [Fact]
      public void Serialize_ShouldReturnByteArray()
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
      public void Deserialize_ShouldReturnExpectedObject()
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
      public void Serialize_NoContract_ShouldReturnByteArray()
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
      public void Deserialize_NoContract_ShouldReturnExpectedObject()
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
      public void Constructor_WithNullOptions_ShouldThrow()
      {
         Action act = () => _ = new MessageSerializer(null);
         act.Should().Throw<ArgumentNullException>();
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
      public async Task SerializeAsync_ShouldReturnByteArray()
      {
         var data = new SampleData { Name = "Async", Age = 45, DateOfBirth = DateTime.Now };
         var bytes = await this.serializer.SerializeAsync(data);

         bytes.Should().NotBeNull();
         bytes.Should().NotBeEmpty();
      }

      [Fact]
      public async Task DeserializeAsync_ShouldReturnExpectedObject()
      {
         var data = new SampleData { Name = "AsyncBob", Age = 35, DateOfBirth = DateTime.Now };
         var bytes = await this.serializer.SerializeAsync(data);

         var result = await this.serializer.DeserializeAsync<SampleData>(bytes);

         result.Should().NotBeNull();
         result.Name.Should().Be("AsyncBob");
         result.Age.Should().Be(35);
      }

      [Fact]
      public void Serialize_WithInvalidType_ShouldThrow()
      {
         var obj = new UnserializableType();
         var customSerializer = new MessageSerializer(MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));

         Action act = () => customSerializer.Serialize(obj);

         act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unexpected error during serialization of type*");
      }

      [Fact]
      public void Deserialize_WithInvalidData_ShouldThrow()
      {
         var invalidBytes = new byte[] { 0x00, 0x01, 0x02 };
         var customSerializer = new MessageSerializer(MessagePackSerializerOptions.Standard);

         Action act = () => customSerializer.Deserialize<SampleData>(invalidBytes);

         act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unexpected error during deserialization to type*");
      }
   }
}
