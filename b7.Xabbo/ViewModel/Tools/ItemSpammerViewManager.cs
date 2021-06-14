using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.Game;

namespace b7.Xabbo.ViewModel
{
    public class ItemSpammerViewManager : ComponentViewModel
    {
        private readonly ProfileManager _profileManager;
        private readonly RoomManager _roomManager;

        private CancellationTokenSource _cts;

        private HashSet<int> _autoUserIds = new HashSet<int>();
        private HashSet<int> _autoItemIds = new HashSet<int>();

        private bool _canAutofill;
        public bool CanAutofill
        {
            get => _canAutofill;
            private set => Set(ref _canAutofill, value);
        }

        private bool _useAutofill;
        public bool UseAutofill
        {
            get => _useAutofill;
            set
            {
                if (Set(ref _useAutofill, value) && value)
                {
                    _autoItemIds.Clear();
                    _autoUserIds.Clear();

                    FurniId = null;
                    UserIdsText = "";
                    _handItemIdsText = "";
                }
            }
        }

        private int? _furniId;
        public int? FurniId
        {
            get => _furniId;
            set => Set(ref _furniId, value);
        }

        private int _delay = 500;
        public int Delay
        {
            get => _delay;
            set => Set(ref _delay, value);
        }

        private string _userIdsText;
        public string UserIdsText
        {
            get => _userIdsText;
            set => Set(ref _userIdsText, value);
        }

        private string _handItemIdsText;
        public string HandItemIdsText
        {
            get => _handItemIdsText;
            set => Set(ref _handItemIdsText, value);
        }

        private bool _isExclusive = true;
        public bool IsExclusive
        {
            get => _isExclusive;
            set => Set(ref _isExclusive, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => Set(ref _isRunning, value);
        }

        public ICommand StartStopCommand { get; }

        public ItemSpammerViewManager(IInterceptor interceptor,
            ProfileManager profileManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _profileManager = profileManager;
            _roomManager = roomManager;

            StartStopCommand = new RelayCommand(OnStartStop);
        }

        private async Task InitializeAsync()
        {
#if PRO
            base.OnInitialize();

            if (!_profileManager.IsAvailable ||
                !_roomManager.IsAvailable ||
                !_entityManager.IsAvailable ||
                Out.ToggleFloorItem <= 0 ||
                Out.RoomUserGiveHandItem <= 0)
            {
                return;
            }

            try
            {
                await _profileManager.GetUserDataAsync();
            }
            catch { return; }

            CanAutofill = IsAttached;

            IsAvailable = true;
#endif
        }

        private async void OnStartStop()
        {
#if PRO
            if (IsRunning)
            {
                _cts?.Cancel();
                return;
            }

            if (_profileManager.UserData == null ||
                !_roomManager.IsInRoom ||
                !_furniId.HasValue ||
                _delay <= 0)
                return;

            var userIds = new HashSet<int>();
            foreach (string line in UserIdsText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(line, out int id))
                {
                    Dialog.ShowError($"Invalid user id: {line}");
                    return;
                }
                userIds.Add(id);
            }

            var excludedHandItemIds = new HashSet<int>();
            foreach (string line in HandItemIdsText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(line, out int id))
                {
                    Dialog.ShowError($"Invalid hand item id: {line}");
                    return;
                }
                excludedHandItemIds.Add(id);
            }

            try
            {
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                UseAutofill = false;
                IsRunning = true;

                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(50, token);
                    foreach (var user in _entityManager.Users)
                    {
                        if (excludedHandItemIds.Contains(user.HandItem) ||
                            user.Id == _profileManager.UserData.Id ||
                            (IsExclusive == userIds.Contains(user.Id)))
                            continue;

                        await Task.Delay(Delay, token);
                        await SendAsync(Out.ToggleFloorItem, FurniId, 0);
                        await Task.Delay(Delay, token);
                        await SendAsync(Out.RoomUserGiveHandItem, user.Id);
                    }
                }
            }
            catch { }
            finally
            {
                _cts.Dispose();
                _cts = null;

                IsRunning = false;
            }
#endif
        }

#if PRO
        [InterceptIn(nameof(Incoming.RoomUserHandItem))]
        private void InRoomUserHandItem(InterceptArgs e)
        {
            if (!UseAutofill || IsRunning || (_profileManager.UserData == null) || !_roomManager.IsInRoom)
                return;

            int index = e.Packet.ReadInt();
            var user = _entityManager.GetEntityByIndex<IRoomUser>(index);
            if (user == null || user.Id != _profileManager.UserData.Id)
                return;
            int handItemId = e.Packet.ReadInt();
            _autoItemIds.Add(handItemId);

            HandItemIdsText = string.Join("\r\n", _autoItemIds);
        }

        [InterceptOut(nameof(Outgoing.RequestWearingBadges))]
        private void OutRequestWearingBadges(InterceptArgs e)
        {
            if (!UseAutofill || IsRunning || !_roomManager.IsInRoom)
                return;

            int userId = e.Packet.ReadInt();

            _autoUserIds.Add(userId);
            UserIdsText = string.Join("\r\n", _autoUserIds);
        }

        [InterceptOut(nameof(Outgoing.ToggleFloorItem))]
        private void OutToggleFloorItem(InterceptArgs e)
        {
            if (!UseAutofill || _isRunning || !_roomManager.IsInRoom)
                return;

            FurniId = e.Packet.ReadInt();
        }
#endif
    }
}
