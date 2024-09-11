using System;

using Xabbo.Core.GameData;

namespace Xabbo.Ext.Events;

public class GameDataEventArgs : EventArgs
{
    public GameDataType Type { get; }

    public GameDataEventArgs(GameDataType type)
    {
        Type = type;
    }
}
