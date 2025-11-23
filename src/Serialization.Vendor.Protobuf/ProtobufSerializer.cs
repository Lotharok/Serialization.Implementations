using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

      // Cache to track which types we've already processed and avoid unnecessary locks
      private readonly ConcurrentDictionary<Type, byte> processedTypes = new ConcurrentDictionary<Type, byte>();

      // Lock to ensure RuntimeTypeModel registration is atomic
      private readonly object registrationLock = new AccessViolationException();

      // To detect circular references during recursive registration
      private readonly ThreadLocal<HashSet<Type>> currentlyProcessing = new ThreadLocal<HashSet<Type>>(() => new HashSet<Type>());

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
         if (this.processedTypes.ContainsKey(type) || this.model.CanSerialize(type))
         {
            return;
         }

         // Auto-register the type
         lock (this.registrationLock)
         {
            if (this.processedTypes.ContainsKey(type) || this.model.CanSerialize(type))
            {
               return;
            }

            try
            {
               this.RegisterTypeRecursively(type);
            }
            finally
            {
               // Clear the tracking for this thread
               this.currentlyProcessing.Value?.Clear();
            }

            // Mark as processed
            this.processedTypes.TryAdd(type, 0);
         }
      }

      private void RegisterTypeRecursively(Type type)
      {
         if (this.IsPrimitiveOrBasic(type) || this.model.CanSerialize(type))
         {
            return;
         }

         // Avoid infinite loops (A -> B -> A)
         if (this.currentlyProcessing.Value.Contains(type))
         {
            return;
         }

         this.currentlyProcessing.Value.Add(type);

         // Handle Collections (List<T>, Arrays, etc.)
         if (this.IsCollectionType(type, out Type elementType))
         {
            if (elementType != null && !this.IsPrimitiveOrBasic(elementType))
            {
               this.RegisterTypeRecursively(elementType);
            }

            return; // Protobuf handles the collection if it knows the element
         }

         try
         {
            // Register the type in the model
            var metaType = this.model.Add(type, applyDefaultBehaviour: false);

            // 1. PROPERTIES: Sort alphabetically for DETERMINISM
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name); // <--- CRITICAL: Sort by name

            int fieldNumber = 1;

            foreach (var property in properties)
            {
               // Recursively register the property type
               this.RegisterTypeRecursively(property.PropertyType);

               // Assign consistent sequential ID
               metaType.Add(fieldNumber++, property.Name);
            }

            // 2. FIELDS: Sort alphabetically
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.IsInitOnly)
                .OrderBy(f => f.Name); // <--- CRITICAL

            foreach (var field in fields)
            {
               this.RegisterTypeRecursively(field.FieldType);
               metaType.Add(fieldNumber++, field.Name);
            }
         }
         catch (Exception ex)
         {
            throw new NotSupportedException(
               $"Type {type.Name} cannot be automatically registered for protobuf serialization. " +
               $"Consider using [ProtoContract] attributes or disable auto-registration. Error: {ex.Message}", ex);
         }
         finally
         {
            this.currentlyProcessing.Value.Remove(type);
         }
      }

      private bool IsCollectionType(Type type, out Type elementType)
      {
         elementType = null;
         if (type.IsArray)
         {
            elementType = type.GetElementType();
            return true;
         }

         if (type.IsGenericType && (
             type.GetGenericTypeDefinition() == typeof(List<>) ||
             type.GetGenericTypeDefinition() == typeof(IList<>) ||
             type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
             type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
         {
            elementType = type.GetGenericArguments().FirstOrDefault();
            return true;
         }

         return false;
      }

      private bool IsPrimitiveOrBasic(Type type)
      {
         return type.IsPrimitive || type.IsEnum ||
                type == typeof(string) || type == typeof(DateTime) ||
                type == typeof(TimeSpan) || type == typeof(Guid) ||
                type == typeof(decimal) || type == typeof(byte[]);
      }
   }
}
