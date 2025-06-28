using System;
using System.IO;
using System.Threading.Tasks;
using Component.Serialization.Contract;

namespace Serialization.Vendor.Protobuf
{
   /// <summary>
   /// Protobuf serializer implementation using protobuf-net library.
   /// Provides binary serialization with compact format and high performance.
   /// Automatically detects if types have [ProtoContract] attributes and falls back
   /// to runtime registration when needed.
   /// </summary>
   /// <remarks>
   /// This serializer provides the best of both worlds:
   /// - High performance for types with [ProtoContract]/[ProtoMember] attributes
   /// - Automatic registration for legacy types without attributes.
   /// </remarks>
   public class ProtobufSerializer : ISerializator<byte[]>
   {
      private readonly ProtoBuf.Meta.RuntimeTypeModel model;

      /// <summary>
      /// Initializes a new instance with the default runtime type model.
      /// </summary>
      public ProtobufSerializer()
          : this(ProtoBuf.Meta.RuntimeTypeModel.Default)
      {
      }

      /// <summary>
      /// Initializes a new instance with a custom runtime type model.
      /// </summary>
      /// <param name="model">Custom runtime type model for advanced scenarios.</param>
      /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
      public ProtobufSerializer(ProtoBuf.Meta.RuntimeTypeModel model)
      {
         this.model = model ?? throw new ArgumentNullException(nameof(model));
      }

      /// <inheritdoc />
      public byte[] Serialize<TValue>(TValue value)
      {
         if (value == null)
         {
            throw new ArgumentNullException(nameof(value));
         }

         try
         {
            // Ensure type can be serialized
            this.EnsureTypeCanBeProcessed<TValue>();
            using var ms = new MemoryStream();
            this.model.Serialize(ms, value);
            return ms.ToArray();
         }
         catch (Exception ex)
         {
            throw new InvalidOperationException(
               $"Failed to serialize object of type {typeof(TValue).Name}. " +
               $"Ensure the type is decorated with [ProtoContract] and [ProtoMember] attributes. " +
               $"Error: {ex.Message}", ex);
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
            // Ensure type can be serialized
            this.EnsureTypeCanBeProcessed<TValue>();
            using var ms = new MemoryStream(buffer);
            var result = this.model.Deserialize<TValue>(ms);
            return result;
         }
         catch (Exception ex)
         {
            throw new InvalidOperationException(
                $"Failed to deserialize buffer to type {typeof(TValue).Name}: {ex.Message}" +
                $"Ensure the type is decorated with [ProtoContract] and [ProtoMember] attributes.", ex);
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
      /// Ensures a type can be processed by the serializer.
      /// For attributed types: verifies they're already registered.
      /// For non-attributed types: auto-registers them if enabled.
      /// </summary>
      /// <typeparam name="T">Type to check/register.</typeparam>
      private void EnsureTypeCanBeProcessed<T>()
      {
         var type = typeof(T);

         // Quick check: if type can already be serialized, we're done
         // Overhead: ~1-5 nanoseconds
         if (this.model.CanSerialize(type))
         {
            return;
         }

         // Auto-register the type
         this.RegisterTypeAutomatically(type);
      }

      /// <summary>
      /// Automatically registers a type for protobuf serialization.
      /// </summary>
      /// <param name="type">Type to register.</param>
      private void RegisterTypeAutomatically(Type type)
      {
         try
         {
            // Register the type with automatic field detection
            var metaType = this.model.Add(type, false);

            // Add all public properties as fields with automatic numbering
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public |
                                              System.Reflection.BindingFlags.Instance);

            int fieldNumber = 1;
            foreach (var property in properties)
            {
               // Skip indexers
               if (property.CanRead && property.CanWrite &&
                   property.GetIndexParameters().Length == 0)
               {
                  metaType.Add(fieldNumber++, property.Name);
               }
            }

            // Also add public fields
            var fields = type.GetFields(System.Reflection.BindingFlags.Public |
                                      System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
               // Skip readonly fields
               if (!field.IsInitOnly)
               {
                  metaType.Add(fieldNumber++, field.Name);
               }
            }
         }
         catch (Exception ex)
         {
            throw new NotSupportedException(
               $"Type {type.Name} cannot be automatically registered for protobuf serialization. " +
               $"Consider using [ProtoContract] attributes or disable auto-registration. Error: {ex.Message}", ex);
         }
      }
   }
}
