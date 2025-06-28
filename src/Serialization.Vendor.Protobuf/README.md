# Serialization.Vendor.Protobuf

High-performance Protocol Buffers serialization implementation using the protobuf-net library. Provides compact binary serialization with automatic type registration capabilities.

## 🎯 Features

- **High Performance**: Optimized binary serialization with excellent speed
- **Compact Format**: Minimal size overhead for serialized data
- **Dual Mode Operation**:
  - High performance for types with `[ProtoContract]` attributes
  - Automatic registration for legacy types without attributes
- **Flexible Type Model**: Support for custom RuntimeTypeModel configurations
- **Async Support**: Built-in asynchronous operations
- **Smart Registration**: Automatic detection and registration of serializable members

## 📦 Installation

```bash
dotnet add package ChacBolay.Serialization.Vendor.Protobuf
```

**Dependencies:**
- protobuf-net (latest stable version)

## 🚀 Quick Start

### Basic Usage

```csharp
using Serialization.Vendor.Protobuf;

// Create serializer
ISerializator<byte[]> serializer = new ProtobufSerializer();

// Serialize
var data = new Person { Name = "John Doe", Age = 30 };
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

### Attributed Types (Recommended)

For maximum performance, decorate your types with Protocol Buffers attributes:

```csharp
[ProtoContract]
public class Person
{
    [ProtoMember(1)]
    public string Name { get; set; }
    
    [ProtoMember(2)]
    public int Age { get; set; }
    
    [ProtoMember(3)]
    public List<string> Emails { get; set; }
}
```

### Non-Attributed Types (Auto-Registration)

The serializer automatically registers types without attributes:

```csharp
public class LegacyPerson
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
}

// Works automatically - no attributes needed!
var buffer = serializer.Serialize(new LegacyPerson 
{ 
    Name = "Jane", 
    Age = 25, 
    BirthDate = DateTime.Now 
});
```

**Auto-Registration Rules:**
- Public read/write properties are included
- Public non-readonly fields are included
- Indexers are skipped
- Field numbers are assigned automatically (starting from 1)

## ⚙️ Configuration

### Default Configuration

```csharp
// Uses RuntimeTypeModel.Default
var serializer = new ProtobufSerializer();
```

### Custom Type Model

```csharp
using ProtoBuf.Meta;

// Create custom model
var model = RuntimeTypeModel.Create();
model.AutoCompile = false;

// Configure specific types
model.Add<MyType>(false)
     .Add(1, nameof(MyType.Property1))
     .Add(2, nameof(MyType.Property2));

// Use custom model
var serializer = new ProtobufSerializer(model);
```

## 📊 Performance Characteristics

### Speed
- **Attributed Types**: ~1-5 nanoseconds overhead for type checking
- **Auto-Registered Types**: Additional one-time registration cost
- **Serialization**: Extremely fast binary encoding
- **Deserialization**: High-speed binary decoding

### Size
- **Compact Format**: Significantly smaller than JSON
- **Variable Length Encoding**: Numbers use only required bytes
- **No Field Names**: Only field numbers are stored

### Memory
- **Low Allocation**: Minimal garbage collection pressure
- **Streaming Support**: Can serialize to/from streams
- **Efficient Buffering**: Optimized memory usage patterns

## 🛡️ Error Handling

### Common Exceptions

```csharp
try
{
    var buffer = serializer.Serialize(data);
}
catch (ArgumentNullException)
{
    // Value was null
}
catch (InvalidOperationException ex)
{
    // Serialization failed - check inner exception for details
    // Message includes guidance about [ProtoContract] attributes
}
catch (NotSupportedException ex)
{
    // Type cannot be auto-registered
    // Consider adding [ProtoContract] attributes
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
    // Deserialization failed - check buffer format and type compatibility
}
```

## 🔧 Advanced Scenarios

### Inheritance Support

```csharp
[ProtoContract]
[ProtoInclude(10, typeof(Employee))]
[ProtoInclude(11, typeof(Customer))]
public abstract class Person
{
    [ProtoMember(1)]
    public string Name { get; set; }
}

[ProtoContract]
public class Employee : Person
{
    [ProtoMember(1)]
    public string Department { get; set; }
}
```

### Collection Support

```csharp
[ProtoContract]
public class Container
{
    [ProtoMember(1)]
    public List<string> Items { get; set; }
    
    [ProtoMember(2)]
    public Dictionary<string, int> Mappings { get; set; }
    
    [ProtoMember(3)]
    public HashSet<int> UniqueNumbers { get; set; }
}
```

### Custom Serialization

```csharp
[ProtoContract]
public class CustomType
{
    [ProtoMember(1)]
    public string Data { get; set; }
    
    // Custom serialization logic
    [ProtoBeforeSerialization]
    void BeforeSerialize() { /* preprocessing */ }
    
    [ProtoAfterDeserialization]
    void AfterDeserialize() { /* postprocessing */ }
}
```

## ⚡ Best Practices

### Performance Tips

1. **Use Attributes When Possible**: Attributed types have better performance
2. **Reuse Serializer Instances**: Create once, use many times
3. **Batch Operations**: Serialize collections rather than individual items
4. **Warm Up**: Call serialization once to initialize type registration

### Type Design

1. **Sequential Field Numbers**: Use 1, 2, 3... for better encoding
2. **Reserved Numbers**: Keep field numbers stable across versions
3. **Optional vs Required**: Use nullable types for optional fields
4. **Avoid Complex Inheritance**: Simple inheritance hierarchies work best

### Versioning

1. **Additive Changes**: Add new fields with higher numbers
2. **Field Deprecation**: Mark fields as `[Obsolete]` instead of removing
3. **Type Evolution**: Use `[ProtoInclude]` for adding subtypes
4. **Backward Compatibility**: Test with old and new versions

## 🧪 Testing Your Types

```csharp
[Test]
public void TestSerialization()
{
    var serializer = new ProtobufSerializer();
    var original = new MyType { /* initialize */ };
    
    // Test round-trip
    var buffer = serializer.Serialize(original);
    var restored = serializer.Deserialize<MyType>(buffer);
    
    // Verify equality
    Assert.AreEqual(original.Property1, restored.Property1);
    Assert.AreEqual(original.Property2, restored.Property2);
}
```

## 📝 Requirements

- .NET Standard 2.0+
- protobuf-net 3.0+

## 🔗 References

- [protobuf-net Documentation](https://protobuf-net.github.io/protobuf-net/)
- [Protocol Buffers Language Guide](https://developers.google.com/protocol-buffers/docs/proto3)
- [Performance Best Practices](https://protobuf-net.github.io/protobuf-net/performance)