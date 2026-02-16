using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace VRCX.Core.Models.GamePlayerPrefs;

public record RegistryKeyValue(
    RegistryValueType type
);

public record RegistryDWordValue(
    int data,
    RegistryValueType type = RegistryValueType.DWord
) : RegistryKeyValue(type);

public record RegistryDoubleInDWordValue(
    double data,
    RegistryValueType type = RegistryValueType.DoubleInDWord
) : RegistryKeyValue(type);

public record RegistryUtf8BinaryValue(
    string data,
    RegistryValueType type = RegistryValueType.Utf8Binary
) : RegistryKeyValue(type);

public enum RegistryValueType
{
    DWord = 4,
    Utf8Binary = 3,
    DoubleInDWord = 100
}

public sealed class RegistryKeyValueJsonConverter : JsonConverter<RegistryKeyValue>
{
    public override RegistryKeyValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        if (!jsonDoc.RootElement.TryGetProperty("type", out var typeProp))
        {
            throw new JsonException("Missing 'type' property.");
        }

        var typeValue = typeProp.GetInt32();
        return typeValue switch
        {
            4 => jsonDoc.RootElement.Deserialize<RegistryDWordValue>(
                RegistryKeyValueJsonContext.Default.RegistryDWordValue
            ),
            3 => jsonDoc.RootElement.Deserialize<RegistryUtf8BinaryValue>(
                RegistryKeyValueJsonContext.Default.RegistryUtf8BinaryValue
            ),
            100 => jsonDoc.RootElement.Deserialize<RegistryDoubleInDWordValue>(
                RegistryKeyValueJsonContext.Default.RegistryDoubleInDWordValue
            ),
            _ => throw new JsonException($"Unknown 'type' value: {typeValue}")
        };
    }

    public override void Write(Utf8JsonWriter writer, RegistryKeyValue value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), RegistryKeyValueJsonContext.Default);
    }
}