using System.Text.RegularExpressions;

using Xabbo.Messages.Flash;
using Xabbo.Core.GameData;

namespace Xabbo.Command.Modules;

[CommandModule(SupportedClients = ~ClientType.Shockwave)]
public sealed partial class EffectCommands : CommandModule
{
    [GeneratedRegex(@"^fx_(\d+)$")]
    private static partial Regex RegexEffect();

    private readonly IGameDataManager _gameDataManager;

    private bool _isReady, _isFaulted;

    private readonly Dictionary<int, string> _effectNames = new();

    public EffectCommands(IGameDataManager gameDataManager)
    {
        _gameDataManager = gameDataManager;

        _gameDataManager.Loaded += OnGameDataLoaded;
        _gameDataManager.LoadFailed += OnGameDataLoadFailed;
    }

    protected override void OnInitialize()
    {
        IsAvailable = true;
    }

    private void OnGameDataLoadFailed(Exception ex)
    {
        _isFaulted = true;
    }

    private void OnGameDataLoaded()
    {
        var texts = _gameDataManager.Texts ?? throw new Exception("Failed to load game data.");

        foreach (var (key, value) in texts)
        {
            var match = RegexEffect().Match(key);
            if (match.Success)
            {
                int effectId = int.Parse(match.Groups[1].Value);
                _effectNames[effectId] = value;
            }
        }

        _isReady = true;
    }

    private List<(int Id, string Name)> FindEffects(string searchText)
    {
        searchText = searchText.ToLower();

        return _effectNames
            .Where(x => x.Value.ToLower().Contains(searchText))
            .OrderBy(x => Math.Abs(x.Value.Length - searchText.Length))
            .Select(x => (x.Key, x.Value))
            .ToList();
    }

    private void EnableMatchingEffect(CommandArgs args, bool activate)
    {
        if (_isFaulted)
        {
            ShowMessage($"This command is unavailable (external texts failed to load)");
            return;
        }
        else if (!_isReady)
        {
            ShowMessage($"This command is currently unavailable (loading external texts)");
            return;
        }

        string searchText = string.Join(" ", args);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            Ext.Send(Out.AvatarEffectSelected, -1);
            return;
        }

        var matches = FindEffects(searchText);
        if (matches.Count > 0)
        {
            if (activate)
                Ext.Send(Out.AvatarEffectActivated, matches[0].Id);
            Ext.Send(Out.AvatarEffectSelected, matches[0].Id);
        }
        else
        {
            ShowMessage($"No effects matching '{searchText}' found");
        }
    }

    [Command("fxa")]
    public Task OnActivateEffect(CommandArgs args)
    {
        EnableMatchingEffect(args, true);
        return Task.CompletedTask;
    }

    [Command("fx")]
    public Task OnEnableEffect(CommandArgs args)
    {
        EnableMatchingEffect(args, false);
        return Task.CompletedTask;
    }

    [Command("dropfx")]
    public Task OnDropEffect(CommandArgs args)
    {
        Ext.Send(Out.Chat, ":yyxxabxa", 0, -1);
        Ext.Send(Out.Shout, ":yyxxabxa", 0);
        return Task.CompletedTask;
    }
}
