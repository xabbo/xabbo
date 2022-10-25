using System;
using System.Threading.Tasks;

namespace b7.Xabbo.Services;

public interface IUiContext
{
    bool IsSynchronized { get; }
    void Invoke(Action callback);
    Task InvokeAsync(Action callback);
}
