using System.Text.Json.Serialization;

namespace Xabbo.Web.Serialization;

[JsonSourceGenerationOptions(
    NumberHandling = JsonNumberHandling.AllowReadingFromString
)]
[JsonSerializable(typeof(Dto.MarketplaceItemStats))]
public partial class JsonWebContext : JsonSerializerContext;