# Serialization.Vendor.MessagePack

Ultra-high-performance MessagePack serialization implementation using MessagePack-CSharp library. Provides the fastest binary serialization with zero configuration requirements.

## 🎯 Features

- **Exceptional Performance**: Fastest serialization in the library
- **Zero Configuration**: Works with POCOs without any attributes
- **Attribute Support**: Enhanced performance with `[MessagePackObject]` attributes
- **Composite Resolution**: Automatic fallback from attributed to contractless types
- **Rich Type Support**: Supports most .NET types out of the box
- **Cross-Platform**: Compatible with MessagePack implementations in other languages

## 📦 Installation

```bash
dotnet add package ChacBolay.Serialization.Vendor.MessagePack
```

**Dependencies:**
- MessagePack (latest stable version)

## 🚀 Quick Start

### Basic Usage (No Attributes Required)

```csharp
using Serialization.Vendor.MessagePack;

// Create serializer
ISerializator<byte[]> serializer = new MessageSerializer();

// Works with any POCO - no attributes needed!
var data = new Person 
{ 
    Name = "John Doe", 
    Age = 30, 
    Email = "john@example.com" 
};

// Serialize
byte[] buffer = serializer.Serialize(data);

// Deserialize
var restored = serializer.Deserialize<Person>(buffer);
```

### Async Operations

```csharp
// Async serialization
byte[] buffer = await serializer.SerializeAsync(data);

// Async deserialization
var restored = await serializer.DeserializeAsync<Person>(buffer);
```

## 🏗️ Type Support

### Contractless Types (Zero Configuration)

MessagePack works seamlessly with plain C# objects:

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public List<string> Hobbies { get; set; }
    public Address HomeAddress { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}

// Serializes automatically - no setup required!
```

### Attributed Types (Enhanced Performance)

For maximum performance, use MessagePack attributes:

```csharp
[MessagePackObject]
public class OptimizedPerson
{
    [Key(0)]
    public string Name { get; set; }
    
    [Key(1)]
    public int Age { get; set; }
    
    [Key(2)]
    public DateTime BirthDate { get; set; }
    
    [IgnoreMember]
    public string InternalData { get; set; } // Won't be serialized
}
```

## ⚙️ Configuration

### Default Configuration

```csharp
// Uses composite resolver (attributed + contractless)
var serializer = new MessageSerializer();
```

The default configuration uses a composite resolver that:
1. First tries `StandardResolver` (for attributed types)
2. Falls back to `ContractlessStandardResolver` (for plain POCOs)

### Custom Options

```csharp
using MessagePack;
using MessagePack.Resolvers;

// Custom resolver
var resolver = CompositeResolver.Create(
    GeneratedResolver.Instance,      // Your generated resolver
    StandardResolver.Instance,       // Attributed types
    ContractlessStandardResolver.Instance  // Plain POCOs
);

var options = MessagePackSerializerOptions.Standard
    .WithResolver(resolver)
    .WithCompression(MessagePackCompression.Lz4BlockArray);

var serializer = new MessageSerializer(options);
```

### Compression Options

```csharp
// LZ4 compression for smaller payloads
var options = MessagePackSerializerOptions.Standard
    .WithCompression(MessagePackCompression.Lz4BlockArray);

var serializer = new MessageSerializer(options);
```

## 📊 Performance Characteristics

### Speed Benchmarks
- **Fastest Binary Format**: Typically 5-10x faster than JSON
- **Minimal Overhead**: Extremely low serialization overhead
- **High Throughput**: Excellent for high-frequency operations
- **Memory Efficient**: Low allocation during serialization

### Size Efficiency
- **Compact Binary**: Smaller than JSON, competitive with Protobuf
- **Schema-less**: No schema definition required
- **Efficient Encoding**: Optimized for common data patterns

## 🛡️ Error Handling

### Serialization Errors

```csharp
try
{
    var buffer = serializer.Serialize(data);
}
catch (ArgumentNullException)
{
    // Value was null (if null handling is strict)
}
catch (InvalidOperationException ex)
{
    // Unexpected serialization error
    // Check inner exception for MessagePack-specific details
}
```

### Deserialization Errors

```csharp
try
{
    var result = serializer.Deserialize<MyType>(buffer);
}
catch (ArgumentNullException)
{
    // Buffer was null
}
catch (ArgumentException)
{
    // Buffer was empty
}
catch (InvalidOperationException ex)
{
    // Deserialization failed
    // Possible causes: corrupted data, type mismatch
}
```

## 🔧 Advanced Features

### Collection Support

MessagePack excellently handles all .NET collections:

```csharp
public class CollectionExample
{
    public List<string> Items { get; set; }
    public Dictionary<string, int> Mappings { get; set; }
    public HashSet<int> UniqueNumbers { get; set; }
    public Queue<DateTime> Timeline { get; set; }
    public int[] Numbers { get; set; }
}
```

### Inheritance and Polymorphism

```csharp
[MessagePackObject]
[Union(0, typeof(Dog))]
[Union(1, typeof(Cat))]
public abstract class Animal
{
    [Key(0)]
    public string Name { get; set; }
}

[MessagePackObject]
public class Dog : Animal
{
    [Key(1)]
    public string Breed { get; set; }
}

[MessagePackObject]
public class Cat : Animal
{
    [Key(1)]
    public bool IsIndoor { get; set; }
}
```

### Custom Type Handling

```csharp
// Custom formatter for special types
public class CustomTypeFormatter : IMessagePackFormatter<CustomType>
{
    public void Serialize(ref MessagePackWriter writer, CustomType value, MessagePackSerializerOptions options)
    {
        // Custom serialization logic
    }

    public CustomType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Custom deserialization logic
    }
}

// Register custom formatter
var resolver = CompositeResolver.Create(
    new IMessagePackFormatter[] { new CustomTypeFormatter() },
    StandardResolver.Instance
);
```

### DateTime Handling

```csharp
// Configure DateTime format
var options = MessagePackSerializerOptions.Standard
    .WithResolver(CompositeResolver.Create(
        NativeDateTimeResolver.Instance,  // Binary DateTime
        StandardResolver.Instance
    ));
```

## ⚡ Best Practices

### Performance Optimization

1. **Reuse Serializer Instances**: Create once, use many times
2. **Use Attributes for Hot Paths**: Attributed types are fastest
3. **Batch Serialization**: Serialize collections rather than individual items
4. **Avoid Boxing**: Use generic methods to prevent boxing overhead

### Type Design

1. **Simple Properties**: Public get/set properties work best
2. **Avoid Complex Constructors**: Parameterless constructors preferred
3. **Collection Initialization**: Initialize collections in constructor
4. **Immutable Types**: Consider readonly properties with constructor injection

### Memory Management

1. **Buffer Reuse**: Consider pooling byte arrays for high-frequency operations
2. **Stream Serialization**: Use streams for large objects
3. **Compression**: Enable compression for network scenarios

## 🧪 Testing and Validation

```csharp
[Test]
public void TestMessagePackSerialization()
{
    var serializer = new MessageSerializer();
    var original = new MyType 
    { 
        Name = "Test",
        Value = 42,
        Items = new[] { "a", "b", "c" }
    };
    
    // Test round-trip
    var buffer = serializer.Serialize(original);
    var restored = serializer.Deserialize<MyType>(buffer);
    
    // Verify
    Assert.AreEqual(original.Name, restored.Name);
    Assert.AreEqual(original.Value, restored.Value);
    CollectionAssert.AreEqual(original.Items, restored.Items);
}

[Test]
public void TestPerformance()
{
    var serializer = new MessageSerializer();
    var data = GenerateTestData();
    
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 10000; i++)
    {
        var buffer = serializer.Serialize(data);
        var restored = serializer.Deserialize<TestData>(buffer);
    }
    
    stopwatch.Stop();
    Console.WriteLine($"10,000 round-trips: {stopwatch.ElapsedMilliseconds}ms");
}
```

## 🌐 Cross-Platform Compatibility

MessagePack is supported across many platforms and languages:

- **C#/.NET**: MessagePack-CSharp (this implementation)
- **JavaScript**: msgpack-lite, @msgpack/msgpack
- **Python**: msgpack-python
- **Java**: msgpack-java
- **Go**: msgpack-go
- **Rust**: rmp-serde

The binary format is identical across all implementations.

## 📝 Requirements

- .NET Standard 2.0+
- MessagePack 2.3.0+

## 🔗 References

- [MessagePack-CSharp Documentation](https://github.com/neuecc/MessagePack-CSharp)
- [MessagePack Format Specification](https://msgpack.org/)
- [Performance Benchmarks](https://github.com/neuecc/MessagePack-CSharp#performance)