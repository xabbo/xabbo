using Xabbo.Models.Enums;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class AppPathService : IAppPathProvider
{
    private static string GetAppDataFilePath(string fileName) => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "xabbo",
        fileName
    );

    public string GetPath(AppPathKind kind) => kind switch
    {
        AppPathKind.Settings => GetAppDataFilePath("config.json"),
        AppPathKind.Wardrobe => GetAppDataFilePath("wardrobe.json"),
        AppPathKind.RoomPasswords => GetAppDataFilePath("passwords.json"),
        _ => throw new Exception($"Unsupported app path kind: '{kind}'."),
    };
}