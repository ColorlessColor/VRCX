using System.Text.Json.Nodes;

namespace VRCX.Core.Utils;

public static class JsonUtils
{
    public static JsonValue? GetJsonValueFromBaseType(object? baseTypeValue)
    {
        if (TryGetJsonValueFromBaseType(baseTypeValue, out var value))
        {
            return value;
        }

        throw new ArgumentException(
            $"Unsupported type for JSON conversion: {baseTypeValue?.GetType().FullName ?? "null"}");
    }

    public static bool TryGetJsonValueFromBaseType(object? baseTypeValue, out JsonValue? value)
    {
        switch (baseTypeValue)
        {
            case null:
                value = null;
                return true;
            case int intValue:
                value = JsonValue.Create(intValue);
                return true;
            case uint uintValue:
                value = JsonValue.Create(uintValue);
                return true;
            case sbyte sbyteValue:
                value = JsonValue.Create(sbyteValue);
                return true;
            case byte byteValue:
                value = JsonValue.Create(byteValue);
                return true;
            case short shortValue:
                value = JsonValue.Create(shortValue);
                return true;
            case ushort ushortValue:
                value = JsonValue.Create(ushortValue);
                return true;
            case long longValue:
                value = JsonValue.Create(longValue);
                return true;
            case ulong ulongValue:
                value = JsonValue.Create(ulongValue);
                return true;
            case float floatValue:
                value = JsonValue.Create(floatValue);
                return true;
            case double doubleValue:
                value = JsonValue.Create(doubleValue);
                return true;
            case decimal decimalValue:
                value = JsonValue.Create(decimalValue);
                return true;
            case bool boolValue:
                value = JsonValue.Create(boolValue);
                return true;
            case string stringValue:
                value = JsonValue.Create(stringValue);
                return true;
        }

        value = null;
        return false;
    }
}