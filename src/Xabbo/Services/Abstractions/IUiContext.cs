namespace Xabbo.Services.Abstractions;

public interface IUiContext
{
    bool IsSynchronized { get; }
    void Invoke(Action callback);
    Task InvokeAsync(Action callback);
}
