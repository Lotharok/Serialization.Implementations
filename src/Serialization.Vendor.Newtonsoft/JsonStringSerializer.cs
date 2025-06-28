using System;
using System.Threading.Tasks;
using Component.Serialization.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Serialization.Vendor.Newtonsoft
{
   /// <summary>
   /// JSON serializer implementation using Newtonsoft.Json library.
   /// Provides string-based serialization with ISO date format handling.
   /// </summary>
   public class JsonStringSerializer : ISerializator<string>
   {
      private readonly JsonSerializerSettings settings;

      /// <summary>
      /// Initializes a new instance with default settings (ISO date format).
      /// </summary>
      public JsonStringSerializer()
         : this(CreateDefaultSettings())
      {
      }

      /// <summary>
      /// Initializes a new instance with custom serialization settings.
      /// </summary>
      /// <param name="settings">Custom JSON serialization settings.</param>
      /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
      public JsonStringSerializer(JsonSerializerSettings settings)
      {
         this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
      }

      /// <inheritdoc />
      public string Serialize<TValue>(TValue value)
      {
         try
         {
            return JsonConvert.SerializeObject(value, this.settings);
         }
         catch (Exception ex)
         {
            throw new InvalidOperationException(
               $"Unexpected error during serialization of type {typeof(TValue).Name}: {ex.Message}", ex);
         }
      }

      /// <inheritdoc />
      public TValue Deserialize<TValue>(string buffer)
      {
         if (buffer == null)
         {
            throw new ArgumentNullException(nameof(buffer));
         }

         if (string.IsNullOrWhiteSpace(buffer))
         {
            throw new ArgumentException("Buffer cannot be empty or whitespace.", nameof(buffer));
         }

         try
         {
            var result = JsonConvert.DeserializeObject<TValue>(buffer, this.settings);
            return result;
         }
         catch (Exception ex)
         {
            throw new InvalidOperationException(
               $"Unexpected error during deserialization to type {typeof(TValue).Name}: {ex.Message}", ex);
         }
      }

      /// <inheritdoc />
      public async Task<string> SerializeAsync<TValue>(TValue value)
      {
         // Para JSON, la serialización es generalmente CPU-bound
         // Usamos Task.Run para no bloquear el hilo actual
         return await Task.Run(() => this.Serialize(value)).ConfigureAwait(false);
      }

      /// <inheritdoc />
      public async Task<TValue> DeserializeAsync<TValue>(string buffer)
      {
         // Para JSON, la deserialización es generalmente CPU-bound
         // Usamos Task.Run para no bloquear el hilo actual
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