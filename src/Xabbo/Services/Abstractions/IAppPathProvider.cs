using Xabbo.Models.Enums;

namespace Xabbo.Services.Abstractions;

public interface IAppPathProvider
{
    string GetPath(AppPathKind kind);
}
