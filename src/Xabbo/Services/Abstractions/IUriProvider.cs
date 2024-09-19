namespace Xabbo.Services.Abstractions;

public interface IUriProvider<TEndpoints>
    where TEndpoints : Enum
{
    string Host { get; set; }
    Uri this[TEndpoints endpoint] { get; }
    Uri GetUri(TEndpoints endpoint, object? parameters = null);
}
