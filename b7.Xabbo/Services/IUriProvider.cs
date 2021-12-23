using System;

namespace b7.Xabbo.Services
{
    public interface IUriProvider<TEndpoints>
        where TEndpoints : Enum
    {
        string Domain { get; set; }
        Uri this[TEndpoints endpoint] { get; }
        Uri GetUri(TEndpoints endpoint, object? parameters = null, string? domain = null);
    }
}
