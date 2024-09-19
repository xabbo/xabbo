using System.Text.Json.Serialization;

using Xabbo.Ext.Model;

namespace Xabbo.Ext.Serialization;

[JsonSerializable(typeof(FigureModel))]
[JsonSerializable(typeof(List<FigureModel>))]
[JsonSerializable(typeof(IEnumerable<FigureModel>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext;