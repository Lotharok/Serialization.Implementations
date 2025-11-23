using System;
using System.Threading.Tasks;
using Component.Serialization.Contract;
using MessagePack;
using MessagePack.Resolvers;

namespace Serialization.Vendor.MessagePack
{
   /// <summary>
   /// MessagePack serializer implementation using MessagePack-CSharp library.
   /// Provides binary serialization with excellent performance and no attribute requirements.
   /// </summary>
   /// <remarks>
   /// MessagePack offers superior performance compared to JSON and doesn't require
   /// attribute decoration like Protobuf. Supports most .NET types out of the box.
   /// </remarks>
   public class MessageSerializer : ISerializator<byte[]>
   {
      private readonly MessagePackSerializerOptions options;

      /// <summary>
      /// Initializes a new instance with contractless resolver (works with and without attributes).
      /// </summary>
      public MessageSerializer()
         : this(CreateDefaultOptions())
      {
      }

      /// <summary>
      /// Initializes a new instance with custom serialization options.
      /// </summary>
      /// <param name="options">Custom MessagePack serialization options.</param>
      /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
      public MessageSerializer(MessagePackSerializerOptions options)
      {
         this.options = options ?? throw new ArgumentNullException(nameof(options));
      }

      /// <inheritdoc />
      public byte[] Serialize<TValue>(TValue value)
      {
         try
         {
            return MessagePackSerializer.Serialize(value, this.options);
         }
         catch (Exception ex)
         {
            throw new InvalidOperationException(
                $"Unexpected error during serialization of type {typeof(TValue).Name}: {ex.Message}", ex);
         }
      }

      /// <inheritdoc />
      public TValue Deserialize<TValue>(byte[] buffer)
      {
         if (buffer == null)
         {
            throw new ArgumentNullException(nameof(buffer));
         }

         if (buffer.Length == 0)
         {
            throw new ArgumentException("Buffer cannot be empty.", nameof(buffer));
         }

         try
         {
            return MessagePackSerializer.Deserialize<TValue>(buffer, this.options);
         }
         catch (Exception ex)
         {
            throw new InvalidOperationException(
                $"Unexpected error during deserialization to type {typeof(TValue).Name}: {ex.Message}", ex);
         }
      }

      /// <inheritdoc />
      public async Task<byte[]> SerializeAsync<TValue>(TValue value)
      {
         return await Task.Run(() => this.Serialize(value)).ConfigureAwait(false);
      }

      /// <inheritdoc />
      public async Task<TValue> DeserializeAsync<TValue>(byte[] buffer)
      {
         return await Task.Run(() => this.Deserialize<TValue>(buffer)).ConfigureAwait(false);
      }

      /// <summary>
      /// Creates default options that work with both attributed and non-attributed types.
      /// </summary>
      /// <returns>MessagePack options with contractless resolver.</returns>
      private static MessagePackSerializerOptions CreateDefaultOptions()
      {
         // CompositeResolver permite usar múltiples resolvers en orden de prioridad
         // Primero intenta usar el resolver estándar (para tipos con atributos)
         // Si falla, usa el contractless resolver (para tipos sin atributos)
         return MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
               StandardResolver.Instance,
               ContractlessStandardResolver.Instance))
            .WithCompression(MessagePackCompression.Lz4BlockArray);
      }
   }
}
