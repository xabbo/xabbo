using Xabbo.Core;
using Xabbo.Core.GameData;

namespace Xabbo.ViewModels;

public class FurniInfoViewModel(FurniInfo info)
{
    public FurniInfo Info { get; } = info;

    public string Name => Info.Name;
    public string Identifier => Info.Identifier;
    public ItemType Type => Info.Type;
    public int Kind => Info.Kind;
    public string Line => Info.Line;
    public string Category => Info.CategoryName;
    public string TypeKind => $"{Type}/{Kind}";
}
