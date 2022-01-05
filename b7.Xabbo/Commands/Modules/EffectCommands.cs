using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xabbo.Core.GameData;

namespace b7.Xabbo.Commands
{
    public class EffectCommands : CommandModule
    {
        private static readonly Regex _regexEffect = new Regex(@"^fx_(\d+)$", RegexOptions.Compiled);

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
                var match = _regexEffect.Match(key);
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
                Send(Out.UseAvatarEffect, -1);
                return;
            }

            var matches = FindEffects(searchText);
            if (matches.Count > 0)
            {
                if (activate)
                    Send(Out.ActivateAvatarEffect, matches[0].Id);
                Send(Out.UseAvatarEffect, matches[0].Id);
            }
            else
            {
                ShowMessage($"No effects matching '{searchText}' found");
            }
        }

        [Command("fxa")]
        protected Task OnActivateEffect(CommandArgs args)
        {
            EnableMatchingEffect(args, true);
            return Task.CompletedTask;
        }

        [Command("fx")]
        protected Task OnEnableEffect(CommandArgs args)
        {
            EnableMatchingEffect(args, false);
            return Task.CompletedTask;
        }

        [Command("dropfx")]
        protected Task OnDropEffect(CommandArgs args)
        {
            Send(Out.Chat, ":yyxxabxa", 0, -1);
            Send(Out.Shout, ":yyxxabxa", 0);
            return Task.CompletedTask;
        }
    }
}
