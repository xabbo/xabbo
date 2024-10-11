using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using ReactiveUI;

using Xabbo.Core;
using Xabbo.Utility;

namespace Xabbo.ViewModels;

public sealed partial class GiftViewModel : ViewModelBase
{
    [GeneratedRegex(@"^<xt>(?<sender>.+)\s(?<date>\d{2}-\d{2}-\d{4})</xt>(?<message>.*)$", RegexOptions.Singleline)]
    private static partial Regex RegexTrophyMessage();

    public ReactiveCommand<Unit, Unit> PeekCmd { get; }

    public IFloorItem? Item { get; }

    [Reactive] public string? Message { get; set; }
    [Reactive] public string? SenderName { get; set; }
    [Reactive] public string? SenderFigure { get; set; }
    [Reactive] public string? ProductCode { get; set; }
    [Reactive] public string? ExtraParameter { get; set; }

    [Reactive] public string? ItemName { get; set; }
    [Reactive] public string? ItemIdentifier { get; set; }
    [Reactive] public string? ItemImageUrl { get; set; }
    private readonly ObservableAsPropertyHelper<string?> _senderImageUrl;
    public string? SenderImageUrl => _senderImageUrl.Value;

    [Reactive] public bool IsTrophy { get; set; }
    [Reactive] public string? TrophyMessage { get; set; }
    [Reactive] public string? TrophyDate { get; set; }

    [Reactive] public bool IsPeeking { get; set; }

    public GiftViewModel()
    {
        PeekCmd = ReactiveCommand.Create(Peek);

        _senderImageUrl = this.WhenAnyValue(x => x.SenderFigure)
            .Select(figure =>
                !string.IsNullOrWhiteSpace(figure)
                ? UrlHelper.AvatarImageUrl(
                    figure: figure,
                    headOnly: true,
                    direction: 2
                )
                : null
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.SenderImageUrl);
    }

    public GiftViewModel(IFloorItem item) : this()
    {
        Item = item;

        if (item.Data is MapData map)
        {
            if (map.TryGetValue("PURCHASER_NAME", out string? purchaserName))
                SenderName = purchaserName;

            if (map.TryGetValue("MESSAGE", out string? message))
                Message = message;

            if (map.TryGetValue("PURCHASER_FIGURE", out string? figureString))
                SenderFigure = figureString;

            if (map.TryGetValue("PRODUCT_CODE", out string? productCode))
                ProductCode = productCode;

            if (map.TryGetValue("EXTRA_PARAM", out string? extraParam))
                ExtraParameter = extraParam;

            if (extraParam is not null)
            {
                var match = RegexTrophyMessage().Match(extraParam);
                if (match.Success)
                {
                    IsTrophy = true;
                    TrophyMessage = match.Groups["message"].Value;
                    TrophyDate = match.Groups["date"].Value;
                }
            }
        }
    }

    void Peek()
    {
        IsPeeking = true;
    }
}
