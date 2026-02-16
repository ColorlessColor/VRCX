using System.Text.Json.Serialization;

namespace VRCX.Core.Models.GamePlayerPrefs;

[JsonSerializable(typeof(RegistryKeyValue))]
[JsonSerializable(typeof(RegistryDWordValue))]
[JsonSerializable(typeof(RegistryDoubleInDWordValue))]
[JsonSerializable(typeof(RegistryUtf8BinaryValue))]
[JsonSerializable(typeof(Dictionary<string, RegistryKeyValue>))]
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters = [typeof(RegistryKeyValueJsonConverter)]
)]
public sealed partial class RegistryKeyValueJsonContext : JsonSerializerContext;