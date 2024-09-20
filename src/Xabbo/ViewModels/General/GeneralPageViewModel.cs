using System.Reactive.Linq;
using System.Diagnostics;
using Splat;
using ReactiveUI;
using HanumanInstitute.MvvmDialogs;
using Avalonia.Media;
using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;
using IconSource = FluentAvalonia.UI.Controls.IconSource;

using Xabbo.Extension;
using Xabbo.Messages.Flash;

using Xabbo.Components;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.ViewModels;

public class GeneralPageViewModel : PageViewModel
{
    public override string Header => "General";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.AppGeneric };

    private readonly IConfigProvider<AppConfig> _settingsProvider;
    private readonly IExtension _ext;

    public AppConfig Config => _settingsProvider.Value;

    public GeneralPageViewModel(IConfigProvider<AppConfig> settingsProvider,
        IAppPathProvider appPathProvider,
        IDialogService dialogService,
        IExtension ext)
    {
        _settingsProvider = settingsProvider;
        _ext = ext;

        _settingsProvider
            .WhenAnyValue(x => x.Value)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(AppConfig)));

        this.ObservableForProperty(x => x.TonerColor)
            .Sample(TimeSpan.FromMilliseconds(300))
            .Subscribe(change =>
            {
                var color = change.Value.ToHsl();
                Debug.WriteLine(color.ToString());
                _ext.Send(Out.SetRoomBackgroundColorData,
                    831085267, (int)Math.Round(color.H / 360 * 255), (int)Math.Round(color.S * 255), (int)Math.Round(color.L * 255));
            });
    }

    [Reactive] public Color TonerColor { get; set; } = Colors.Black;

    [Reactive] public bool IsRoomExpanded { get; set; } = true;
    [Reactive] public bool IsMovementExpanded { get; set; } = true;
    [Reactive] public bool IsClickToExpanded { get; set; } = true;
    [Reactive] public bool IsEntryExpanded { get; set; } = true;
    [Reactive] public bool IsChatExpanded { get; set; } = true;
    [Reactive] public bool IsMuteExpanded { get; set; } = true;
    [Reactive] public bool IsFurniExpanded { get; set; } = true;
    [Reactive] public bool IsHandItemExpanded { get; set; } = true;
    [Reactive] public bool IsAlertsExpanded { get; set; } = true;
    [Reactive] public bool IsFlashWindowExpanded { get; set; } = true;
    [Reactive] public bool IsMiscExpanded { get; set; } = true;

    [DependencyInjectionProperty] public ChatComponent? ChatComponent { get; set; }
    [DependencyInjectionProperty] public FurniActionsComponent? Furni { get; set; }
    [DependencyInjectionProperty] public AntiHandItemComponent? HandItem { get; set; }
    [DependencyInjectionProperty] public AntiIdleComponent? AntiIdle { get; set; }
    [DependencyInjectionProperty] public AntiTradeComponent? AntiTrade { get; set; }
    [DependencyInjectionProperty] public AntiTurnComponent? AntiTurn { get; set; }
    [DependencyInjectionProperty] public AntiTypingComponent? AntiTyping { get; set; }
    [DependencyInjectionProperty] public AntiWalkComponent? AntiWalk { get; set; }
    [DependencyInjectionProperty] public ClickThroughComponent? ClickThrough { get; set; }
    [DependencyInjectionProperty] public ClickToComponent? ClickTo { get; set; }
    [DependencyInjectionProperty] public RespectedComponent? Respected { get; set; }
    [DependencyInjectionProperty] public RoomEntryComponent? RoomEntry { get; set; }
    [DependencyInjectionProperty] public DoorbellComponent? Doorbell { get; set; }
    [DependencyInjectionProperty] public FlattenRoomComponent? Flatten { get; set; }
    [DependencyInjectionProperty] public AvatarOverlayComponent? Overlay { get; set; }
    [DependencyInjectionProperty] public AntiHcGiftNotificationComponent? AntiHcNotification { get; set; }
}
