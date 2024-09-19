using System.Text.Json.Serialization;

using Xabbo.Models;
using Xabbo.Configuration;

namespace Xabbo.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(FigureModel))]
[JsonSerializable(typeof(List<FigureModel>))]
[JsonSerializable(typeof(IEnumerable<FigureModel>))]
[JsonSerializable(typeof(Dictionary<long, string>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext;