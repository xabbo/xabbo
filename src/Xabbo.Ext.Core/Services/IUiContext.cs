using System;
using System.Threading.Tasks;

namespace Xabbo.Ext.Services;

public interface IUiContext
{
    bool IsSynchronized { get; }
    void Invoke(Action callback);
    Task InvokeAsync(Action callback);
}
