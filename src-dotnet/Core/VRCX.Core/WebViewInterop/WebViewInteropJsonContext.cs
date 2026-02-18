using System.Text.Json.Serialization;
using VRCX.Core.Models.GamePlayerPrefs;

namespace VRCX.Core.WebViewInterop;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSerializable(typeof(string[][]))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(Dictionary<string, RegistryKeyValue>))]
[JsonSerializable(typeof(Dictionary<string, short>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(Tuple<long, bool, string>))]
public sealed partial class WebViewInteropJsonContext : JsonSerializerContext;