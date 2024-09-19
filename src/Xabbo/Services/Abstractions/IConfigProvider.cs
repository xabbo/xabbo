namespace Xabbo.Services.Abstractions;

public interface IConfigProvider<T>
{
    T Value { get; }

    event Action? Loaded;
    event Action? Saved;

    void Load();
    void Save();
}
