using System;
using System.Text;
using System.Threading.Tasks;
using Component.Serialization.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Serialization.Vendor.Newtonsoft
{
   /// <summary>
   /// JSON serializer implementation using Newtonsoft.Json library with byte array output.
   /// Provides binary-based JSON serialization with UTF-8 encoding.
   /// </summary>
   public class JsonByteArraySerializer : ISerializator<byte[]>
   {
      private readonly JsonSerializerSettings settings;
      private readonly Encoding encoding;

      /// <summary>
      /// Initializes a new instance with default settings (ISO date format, UTF-8 encoding).
      /// </summary>
      public JsonByteArraySerializer()
          : this(CreateDefaultSettings(), Encoding.UTF8)
      {
      }

      /// <summary>
      /// Initializes a new instance with custom settings and UTF-8 encoding.
      /// </summary>
      /// <param name="settings">Custom JSON serialization settings.</param>
      public JsonByteArraySerializer(JsonSerializerSettings settings)
          : this(settings, Encoding.UTF8)
      {
      }

      /// <summary>
      /// Initializes a new instance with custom settings and encoding.
      /// </summary>
      /// <param name="settings">Custom JSON serialization settings.</param>
      /// <param name="encoding">Text encoding to use for byte conversion.</param>
      /// <exception cref="ArgumentNullException">Thrown when settings or encoding is null.</exception>
      public JsonByteArraySerializer(JsonSerializerSettings settings, Encoding encoding)
      {
         this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
         this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
      }

      /// <inheritdoc />
      public byte[] Serialize<TValue>(TValue value)
      {
         try
         {
            var json = JsonConvert.SerializeObject(value, this.settings);
            return this.encoding.GetBytes(json);
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
            var json = this.encoding.GetString(buffer);
            var result = JsonConvert.DeserializeObject<TValue>(json, this.settings);
            return result;
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
      /// Creates default JSON serialization settings with ISO date format.
      /// </summary>
      /// <returns>Default JsonSerializerSettings instance.</returns>
      private static JsonSerializerSettings CreateDefaultSettings()
      {
         return new JsonSerializerSettings
         {
            Converters = { new IsoDateTimeConverter() },
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
         };
      }
   }
}
