﻿using IconSource = FluentAvalonia.UI.Controls.IconSource;

using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

namespace b7.Xabbo.Avalonia.ViewModels;

public class GameDataPageViewModel : PageViewModel
{
    public override string Header => "Game data";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Database };

    public GameDataPageViewModel() { }
}