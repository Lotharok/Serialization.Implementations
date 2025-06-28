# Serialization Library

A comprehensive and high-performance serialization library for .NET applications that provides multiple serialization formats through a unified interface.

## 🎯 Features

- **Unified Interface**: All serializers implement the same `ISerializator<TBuffer>` interface
- **Multiple Formats**: Support for JSON, MessagePack, and Protocol Buffers
- **Async Support**: Built-in asynchronous serialization and deserialization
- **High Performance**: Optimized implementations for each serialization format
- **Flexible Configuration**: Customizable options for each serializer
- **Auto-Registration**: Automatic type registration for Protocol Buffers when needed
- **Attribute-Free**: Works with POCOs without requiring special attributes (MessagePack, JSON)

## 📦 Available Implementations

### Binary Serializers (byte[])
- **[Serialization.Vendor.Protobuf](./src/Serialization.Vendor.Protobuf/README.md)** - Protocol Buffers with protobuf-net
- **[Serialization.Vendor.MessagePack](./src/Serialization.Vendor.MessagePack/README.md)** - MessagePack with MessagePack-CSharp
- **[Serialization.Vendor.Newtonsoft](./src/Serialization.Vendor.Newtonsoft/README.md)** - JSON as byte array with Newtonsoft.Json

### Text Serializers (string)
- **[Serialization.Vendor.Newtonsoft](./src/Serialization.Vendor.Newtonsoft/README.md)** - JSON as string with Newtonsoft.Json

## 🚀 Quick Start

### Installation

Install the packages you need:

```bash
# For Protocol Buffers
dotnet add package ChacBolay.Serialization.Vendor.Protobuf

# For MessagePack
dotnet add package ChacBolay.Serialization.Vendor.MessagePack

# For JSON (Newtonsoft)
dotnet add package ChacBolay.Serialization.Vendor.Newtonsoft
```

### Basic Usage

```csharp
// Choose your serializer
ISerializator<byte[]> serializer = new ProtobufSerializer();
// OR
ISerializator<byte[]> serializer = new MessageSerializer();
// OR
ISerializator<string> serializer = new JsonStringSerializer();

// Serialize
var data = new MyClass { Name = "John", Age = 30 };
var buffer = serializer.Serialize(data);

// Deserialize
var restored = serializer.Deserialize<MyClass>(buffer);

// Async operations
var bufferAsync = await serializer.SerializeAsync(data);
var restoredAsync = await serializer.DeserializeAsync<MyClass>(bufferAsync);
```

## 🏗️ Architecture

### Core Interface

All serializers implement the [`ISerializator<TBuffer>`](https://github.com/Lotharok/Component.Serialization) interface:

```csharp
public interface ISerializator<TBuffer>
{
    TBuffer Serialize<TValue>(TValue value);
    TValue Deserialize<TValue>(TBuffer buffer);
    Task<TBuffer> SerializeAsync<TValue>(TValue value);
    Task<TValue> DeserializeAsync<TValue>(TBuffer buffer);
}
```

### Type Buffer Support

- `ISerializator<byte[]>` - Binary serialization (Protobuf, MessagePack, JSON as bytes)
- `ISerializator<string>` - Text serialization (JSON as string)

## 📊 Performance Comparison

| Serializer | Speed | Size | Attributes Required | Best For |
|------------|-------|------|-------------------|----------|
| **MessagePack** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ❌ | High-performance scenarios |
| **Protobuf** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⚠️ Optional | Cross-platform compatibility |
| **JSON** | ⭐⭐⭐ | ⭐⭐ | ❌ | Human-readable, web APIs |

## 🔧 Advanced Configuration

### Custom Protocol Buffers Model

```csharp
var customModel = ProtoBuf.Meta.RuntimeTypeModel.Create();
// Configure your model...
var serializer = new ProtobufSerializer(customModel);
```

### Custom MessagePack Options

```csharp
var options = MessagePackSerializerOptions.Standard
    .WithResolver(your_custom_resolver);
var serializer = new MessageSerializer(options);
```

### Custom JSON Settings

```csharp
var settings = new JsonSerializerSettings
{
    DateFormatHandling = DateFormatHandling.UnixTimeStamp,
    NullValueHandling = NullValueHandling.Ignore
};
var serializer = new JsonStringSerializer(settings);
```

## 🛡️ Error Handling

All serializers provide consistent error handling:

- `ArgumentNullException` for null inputs
- `ArgumentException` for invalid inputs (empty buffers)
- `InvalidOperationException` for serialization/deserialization failures with detailed error messages

## ⚡ Best Practices

1. **Choose the Right Serializer**:
   - Use **MessagePack** for maximum performance with binary data
   - Use **Protobuf** when you need cross-platform compatibility
   - Use **JSON** when you need human-readable format or web compatibility

2. **Reuse Serializer Instances**: Create serializers once and reuse them for better performance

3. **Handle Exceptions**: Always wrap serialization calls in try-catch blocks

4. **Use Async Methods**: For I/O-bound operations, prefer async methods to avoid blocking

## 📝 Requirements

- .NET Standard 2.0 or higher
- Dependencies vary by implementation (see individual README files)

## 📖 Documentation

For detailed documentation of each implementation, see:

- [Protocol Buffers Implementation](./src/Serialization.Vendor.Protobuf/README.md)
- [MessagePack Implementation](./src/Serialization.Vendor.MessagePack/README.md)
- [Newtonsoft JSON Implementation](./src/Serialization.Vendor.Newtonsoft/README.md)

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests for any improvements.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.