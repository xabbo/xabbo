using System;

using Xabbo.Core;

namespace b7.Xabbo.ViewModel;

public class NavigatorRoomViewModel
{
    public IRoomInfo Info { get; }

    public long Id => Info.Id;
    public string Name => Info.Name;
    public long OwnerId => Info.OwnerId;
    public string OwnerName => Info.OwnerName;

    public string Description => Info.Description;

    public string RenderedName => H.RenderText(Name);
    public string RenderedDescription => H.RenderText(Description);

    public string Url => $"https://habbo-stories-content.s3.amazonaws.com/navigator-thumbnail/hhus/{Id}.png";

    public float Load => Info.MaxUsers > 0 ? (Info.Users / (float)Info.MaxUsers) : 0;

    public bool IsGreen => Load > 0.0f && Load < 0.5f;
    public bool IsYellow => Load >= 0.5f && Load <= 0.9f;
    public bool IsRed => Load > 0.9f;

    public NavigatorRoomViewModel(IRoomInfo info)
    {
        Info = info;
    }
}
