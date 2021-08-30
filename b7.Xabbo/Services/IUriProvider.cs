using System;
using System.Collections.Generic;

namespace b7.Xabbo.Services
{
    public interface IUriProvider<TEndpoints>
        where TEndpoints : Enum
    {
        string Domain { get; }
        Uri this[TEndpoints endpoint] { get; }
        Uri GetUri(TEndpoints endpoint);
        Uri GetUri(TEndpoints endpoint, object parameters);
    }
}
