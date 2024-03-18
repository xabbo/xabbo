using Xabbo.Core;
using Xabbo.Core.GameData;

namespace b7.Xabbo.Avalonia.ViewModels;

public class FurniInfoViewModel
{
    public FurniInfo Info { get; }

    public string Name => Info.Name;
    public string Identifier => Info.Identifier;
    public ItemType Type => Info.Type;
    public int Kind => Info.Kind;
    public string Line => Info.Line;
    public string Category => Info.CategoryName;
    public string TypeKind => $"{Type}/{Kind}";

    public FurniInfoViewModel(FurniInfo info)
    {
        Info = info;
    }
}
