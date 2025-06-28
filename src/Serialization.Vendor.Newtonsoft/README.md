# Serialization.Vendor.Newtonsoft

Robust JSON serialization implementations using Newtonsoft.Json library. Provides both string and binary JSON serialization with comprehensive configuration options and excellent compatibility.

## 🎯 Features

- **Two Output Formats**: String-based and byte array-based JSON serialization
- **Human Readable**: JSON format is easily readable and debuggable
- **Web Compatible**: Perfect for REST APIs and web applications
- **Rich Configuration**: Extensive customization options via JsonSerializerSettings
- **ISO Date Handling**: Built-in ISO 8601 date format support
- **Universal Compatibility**: Works with virtually any .NET type
- **Encoding Flexibility**: Configurable text encoding (UTF-8 default)
- **Async Support**: Full async/await pattern support
- **Error Handling**: Comprehensive exception handling with meaningful error messages

## 📦 Installation

```bash
dotnet add package ChacBolay.Serialization.Vendor.Newtonsoft
```

**Dependencies:**
- Newtonsoft.Json (latest stable version)
- .NET Standard 2.0+

## 🚀 Quick Start

### String-based JSON Serialization

```csharp
using Serialization.Vendor.Newtonsoft;

// Create string serializer
ISerializator<string> serializer = new JsonStringSerializer();

// Serialize to JSON string
var data = new Person { Name = "John Doe", Age = 30, BirthDate = DateTime.UtcNow };
string json = serializer.Serialize(data);
// Result: {"Name":"John Doe","Age":30,"BirthDate":"2024-01-15T10:30:00Z"}

// Deserialize from JSON string
var restored = serializer.Deserialize<Person>(json);
```

### Byte Array JSON Serialization

```csharp
// Create byte array serializer
ISerializator<byte[]> serializer = new JsonByteArraySerializer();

// Serialize to UTF-8 byte array
byte[] buffer = serializer.Serialize(data);

// Deserialize from byte array
var restored = serializer.Deserialize<Person>(buffer);
```

### Async Operations

```csharp
// Async serialization
string json = await serializer.SerializeAsync(data);
byte[] buffer = await byteSerializer.SerializeAsync(data);

// Async deserialization
var restored = await serializer.DeserializeAsync<Person>(json);
var restoredFromBytes = await byteSerializer.DeserializeAsync<Person>(buffer);
```

## 🏗️ Implementations

### JsonStringSerializer

Serializes objects to/from JSON strings with full Newtonsoft.Json feature support.

**Use Cases:**
- Web APIs and REST services
- Configuration files
- Human-readable data storage
- Debugging and logging
- Direct string manipulation scenarios

**Key Features:**
- Direct string output for immediate use
- Perfect for web applications
- Easy debugging and inspection
- Minimal memory overhead for string operations

```csharp
ISerializator<string> serializer = new JsonStringSerializer();

// With custom settings
var customSettings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented
};
ISerializator<string> prettySerializer = new JsonStringSerializer(customSettings);
```

### JsonByteArraySerializer

Serializes objects to/from UTF-8 encoded JSON byte arrays with configurable encoding.

**Use Cases:**
- Network protocols requiring byte arrays
- Binary storage with JSON format
- Integration with byte-based APIs
- Consistent interface with binary serializers
- File storage scenarios
- Stream-based operations

**Key Features:**
- Configurable text encoding (UTF-8, ASCII, Unicode, etc.)
- Consistent byte-based interface
- Perfect for network transmission
- Optimal for file I/O operations

```csharp
ISerializator<byte[]> serializer = new JsonByteArraySerializer();

// With custom encoding
var customSettings = new JsonSerializerSettings();
var encoding = Encoding.UTF8;
ISerializator<byte[]> customSerializer = new JsonByteArraySerializer(customSettings, encoding);
```

## ⚙️ Configuration

### Default Settings

Both serializers come with production-ready default settings:

```csharp
// Default configuration includes:
// - ISO 8601 date format (2024-01-15T10:30:00Z)
// - UTC timezone handling
// - Include null values
// - Include default values
// - No formatting (compact JSON)
// - IsoDateTimeConverter for consistent date handling
```

### Custom JSON Settings

```csharp
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var customSettings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,           // Pretty-print JSON
    NullValueHandling = NullValueHandling.Ignore, // Skip null values
    DateFormatHandling = DateFormatHandling.UnixTimeStamp, // Unix timestamps
    ContractResolver = new CamelCasePropertyNamesContractResolver(), // camelCase
    TypeNameHandling = TypeNameHandling.Auto    // Include type information
};

var serializer = new JsonStringSerializer(customSettings);
```

### Custom Encoding (Byte Array Only)

```csharp
using System.Text;

// Use different encoding
var settings = new JsonSerializerSettings();
var encoding = Encoding.ASCII; // or Encoding.Unicode, UTF32, etc.

var serializer = new JsonByteArraySerializer(settings, encoding);
```

## 📊 Advanced Configuration Examples

### Camel Case Properties

```csharp
var settings = new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

// C# Property: FirstName → JSON: firstName
var serializer = new JsonStringSerializer(settings);
```

### Custom Date Formats

```csharp
var settings = new JsonSerializerSettings
{
    DateFormatString = "yyyy-MM-dd HH:mm:ss",
    DateTimeZoneHandling = DateTimeZoneHandling.Local,
    Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" } }
};
```

### Conditional Serialization

```csharp
var settings = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore,
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    }
};
```

### Custom Converters

```csharp
var settings = new JsonSerializerSettings();
settings.Converters.Add(new StringEnumConverter()); // Enums as strings
settings.Converters.Add(new IsoDateTimeConverter()); // ISO dates
settings.Converters.Add(new DecimalFormatConverter()); // Custom decimal formatting

var serializer = new JsonStringSerializer(settings);
```

### Performance Optimization

```csharp
var settings = new JsonSerializerSettings
{
    // Optimize for performance
    Formatting = Formatting.None,
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore,
    DateParseHandling = DateParseHandling.None,
    FloatParseHandling = FloatParseHandling.Double
};
```

## 🔧 Type Support

### Basic Types

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
    public Guid Id { get; set; }
}

// Serialization works seamlessly
var person = new Person 
{
    Name = "John Doe",
    Age = 30,
    BirthDate = new DateTime(1994, 1, 15),
    Salary = 75000.50m,
    IsActive = true,
    Id = Guid.NewGuid()
};
```

### Collections and Arrays

```csharp
public class CollectionExample
{
    public List<string> Items { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public int[] Numbers { get; set; }
    public HashSet<string> UniqueValues { get; set; }
    public Queue<DateTime> Events { get; set; }
    public Stack<decimal> Values { get; set; }
}

// All collection types are supported
var collections = new CollectionExample
{
    Items = new List<string> { "item1", "item2", "item3" },
    Properties = new Dictionary<string, object> 
    {
        ["key1"] = "value1",
        ["key2"] = 42,
        ["key3"] = true
    },
    Numbers = new[] { 1, 2, 3, 4, 5 },
    UniqueValues = new HashSet<string> { "unique1", "unique2" }
};
```

### Complex Nested Objects

```csharp
public class Order
{
    public int OrderId { get; set; }
    public Customer Customer { get; set; }
    public List<OrderItem> Items { get; set; }
    public Address ShippingAddress { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
}

public class Customer
{
    public int CustomerId { get; set; }
    public string Email { get; set; }
    public ContactInfo Contact { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

### Generic Types

```csharp
public class GenericContainer<T>
{
    public T Value { get; set; }
    public List<T> Items { get; set; }
    public Dictionary<string, T> NamedItems { get; set; }
}

// Works with any serializable type
var stringContainer = new GenericContainer<string>
{
    Value = "test",
    Items = new List<string> { "a", "b", "c" }
};

var intContainer = new GenericContainer<int>
{
    Value = 42,
    Items = new List<int> { 1, 2, 3 }
};
```

## 🛡️ Error Handling

The serializers provide comprehensive error handling with detailed exception information:

### Common Exceptions

```csharp
try
{
    var result = serializer.Serialize(data);
}
catch (InvalidOperationException ex)
{
    // Wraps serialization errors with context
    Console.WriteLine($"Serialization failed: {ex.Message}");
    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
}

try
{
    var data = serializer.Deserialize<MyClass>(json);
}
catch (ArgumentNullException ex)
{
    // Null input validation
    Console.WriteLine("Input cannot be null");
}
catch (ArgumentException ex)
{
    // Empty/invalid input validation
    Console.WriteLine("Input cannot be empty or whitespace");
}
catch (InvalidOperationException ex)
{
    // Deserialization errors with type context
    Console.WriteLine($"Deserialization failed for type {typeof(MyClass).Name}: {ex.Message}");
}
```

### Best Practices for Error Handling

```csharp
public async Task<T> SafeDeserializeAsync<T>(string json)
{
    try
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default(T);
        }

        return await serializer.DeserializeAsync<T>(json);
    }
    catch (InvalidOperationException ex) when (ex.InnerException is JsonException)
    {
        // Handle JSON-specific errors
        _logger.LogError(ex, "Invalid JSON format for type {Type}", typeof(T).Name);
        throw new FormatException($"Invalid JSON format for {typeof(T).Name}", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error deserializing {Type}", typeof(T).Name);
        throw;
    }
}
```

## 🚀 Performance Tips

### 1. Reuse Serializer Instances

```csharp
// ✅ Good - Reuse serializer instances
private static readonly ISerializator<string> _serializer = new JsonStringSerializer();

public string SerializeData(object data)
{
    return _serializer.Serialize(data);
}

// ❌ Avoid - Creating new instances each time
public string SerializeData(object data)
{
    var serializer = new JsonStringSerializer(); // Expensive
    return serializer.Serialize(data);
}
```

### 2. Optimize Settings for Your Use Case

```csharp
// For APIs - optimize for size
var apiSettings = new JsonSerializerSettings
{
    Formatting = Formatting.None,
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore
};

// For debugging - optimize for readability
var debugSettings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    NullValueHandling = NullValueHandling.Include
};
```

### 3. Use Appropriate Async Methods

```csharp
// For CPU-bound serialization of large objects
var largeObject = CreateLargeObject();
var json = await serializer.SerializeAsync(largeObject);

// For small objects, sync methods might be more efficient
var smallObject = CreateSmallObject();
var json = serializer.Serialize(smallObject);
```

## 🔗 Integration Examples

### ASP.NET Core API

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISerializator<string> _serializer;

    public ProductsController(ISerializator<string> serializer)
    {
        _serializer = serializer;
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportProducts()
    {
        var products = await GetProductsAsync();
        var json = await _serializer.SerializeAsync(products);
        
        return File(Encoding.UTF8.GetBytes(json), "application/json", "products.json");
    }
}
```

### Configuration Management

```csharp
public class ConfigurationManager<T> where T : class, new()
{
    private readonly ISerializator<string> _serializer;
    private readonly string _filePath;

    public ConfigurationManager(string filePath)
    {
        _serializer = new JsonStringSerializer(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include
        });
        _filePath = filePath;
    }

    public async Task<T> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new T();

        var json = await File.ReadAllTextAsync(_filePath);
        return _serializer.Deserialize<T>(json);
    }

    public async Task SaveAsync(T config)
    {
        var json = await _serializer.SerializeAsync(config);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
```

### Caching with Byte Arrays

```csharp
public class JsonCacheService
{
    private readonly ISerializator<byte[]> _serializer;
    private readonly IMemoryCache _cache;

    public JsonCacheService(IMemoryCache cache)
    {
        _serializer = new JsonByteArraySerializer();
        _cache = cache;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out byte[] cachedBytes))
        {
            return await _serializer.DeserializeAsync<T>(cachedBytes);
        }

        var data = await factory();
        var bytes = await _serializer.SerializeAsync(data);
        
        _cache.Set(key, bytes, expiration);
        return data;
    }
}
```

## 📝 Migration from System.Text.Json

If you're migrating from System.Text.Json, here are the key differences:

```csharp
// System.Text.Json
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

// Newtonsoft.Json equivalent
var settings = new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    Formatting = Formatting.Indented
};
```

## 🏆 Advantages of Newtonsoft.Json

- **Mature and Stable**: Battle-tested in production environments
- **Rich Feature Set**: Extensive customization options
- **Better DateTime Handling**: Superior date/time serialization control
- **Flexible Type Handling**: Better support for complex inheritance scenarios
- **Custom Converters**: Extensive ecosystem of custom converters
- **Backward Compatibility**: Works with older .NET Framework versions

## 📋 Changelog

### Version 1.0.0
- Initial release with JsonStringSerializer and JsonByteArraySerializer
- Full async support
- Comprehensive error handling
- Configurable encoding support
- ISO 8601 date format by default
