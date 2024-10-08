namespace Xabbo.Services.Abstractions;

public interface ILauncherService
{
    void Launch(string url, Dictionary<string, List<string>>? values = null);
}
