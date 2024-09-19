using System.Text.Json.Serialization;

using Xabbo.Models;

namespace Xabbo.Serialization;

[JsonSerializable(typeof(FigureModel))]
[JsonSerializable(typeof(List<FigureModel>))]
[JsonSerializable(typeof(IEnumerable<FigureModel>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext;