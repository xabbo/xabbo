using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using Xabbo.Messages;
using Xabbo.Interceptor;

using Xabbo.Core.Game;

using b7.Xabbo.Services;

namespace b7.Xabbo.ViewModel
{
    public class BanListViewManager : ComponentViewModel
    {
        private readonly IUiContext _context;
        private readonly RoomManager _roomManager;

        private CancellationTokenSource? _cts;

        private readonly ObservableCollection<BannedUserViewModel> _users = new();
        private readonly ConcurrentDictionary<long, BannedUserViewModel> _userIdMap = new();

        public ICollectionView Users { get; }

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set => Set(ref _isAvailable, value);
        }

        private bool isInRoom;
        public bool IsInRoom
        {
            get => isInRoom;
            set => Set(ref isInRoom, value);
        }

        private bool isWorking;
        public bool IsWorking
        {
            get => isWorking;
            set => Set(ref isWorking, value);
        }

        private bool isUnbanning;
        public bool IsUnbanning
        {
            get => isUnbanning;
            set => Set(ref isUnbanning, value);
        }

        private bool isCancelling;
        public bool IsCancelling
        {
            get => isCancelling;
            set => Set(ref isCancelling, value);
        }

        private string statusText;
        public string StatusText
        {
            get => statusText;
            set => Set(ref statusText, value);
        }

        private string filterText = string.Empty;
        public string FilterText
        {
            get => filterText;
            set
            {
                if (Set(ref filterText, value))
                    RefreshUsers();
            }
        }

        public ICommand CancelCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand UnbanCommand { get; }

        public BanListViewManager(IInterceptor interceptor, IUiContext context, RoomManager roomManager)
            : base(interceptor)
        {
            _context = context;
            _roomManager = roomManager;

            _roomManager.Entered += RoomManager_Entered;
            _roomManager.Left += RoomManager_Left;

            Users = CollectionViewSource.GetDefaultView(_users);
            Users.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Users.Filter = FilterUsers;

            CancelCommand = new RelayCommand(Cancel);
            LoadCommand = new RelayCommand(Load);
            UnbanCommand = new RelayCommand<IList>(Unban);
        }

        protected async Task InitializeAsync()
        {
            // IsAvailable = IsAttached && Headers.AreResolved(this);
        }

        #region - Commands -
        private void Cancel()
        {
            if (!IsWorking || IsCancelling) return;

            IsCancelling = true;
            _cts?.Cancel();
        }

        private async void Load()
        {
            if (IsWorking) return;

            await LoadAsync();
        }

        private async void Unban(IList list)
        {
            if (list == null || IsWorking) return;

            await UnbanAsync(list.OfType<BannedUserViewModel>());
        }
        #endregion

        #region - ViewModel logic -
        private bool FilterUsers(object obj)
        {
            if (!(obj is BannedUserViewModel user))
                return false;

            return user.Name.ToLower().Contains(FilterText.ToLower());
        }

        private void RefreshUsers()
        {
            if (!_context.IsSynchronized)
            {
                _context.InvokeAsync(() => RefreshUsers());
                return;
            }

            Users.Refresh();
        }

        private void Clear()
        {
            if (!_context.IsSynchronized)
            {
                _context.InvokeAsync(() => Clear());
                return;
            }

            _users.Clear();
            _userIdMap.Clear();
        }

        private void AddUser(BannedUserViewModel user)
        {
            if (!_context.IsSynchronized)
            {
                _context.InvokeAsync(() => AddUser(user));
                return;

            }

            _users.Add(user);
        }

        private void RemoveUser(BannedUserViewModel user)
        {
            if (!_context.IsSynchronized)
            {
                _context.InvokeAsync(() => RemoveUser(user));
                return;
            }

            _users.Remove(user);
        }
        #endregion

        #region - Events -
        private void RoomManager_Entered(object? sender, EventArgs e) => IsInRoom = true;
        private void RoomManager_Left(object? sender, EventArgs e)
        {
            IsInRoom = false;

            Cancel();
            Clear();
        }
        #endregion

        #region - Logic -
        [InterceptIn(nameof(Incoming.UserUnbannedFromRoom))]
        protected void HandleRoomUserUnbanned(InterceptArgs e)
        {
            long roomId = e.Packet.ReadLegacyLong();
            if (roomId != _roomManager.CurrentRoomId)
                return;

            int userId = e.Packet.ReadInt();
            if (_userIdMap.TryRemove(userId, out BannedUserViewModel? user))
                RemoveUser(user);
        }

        private async Task LoadAsync() 
        {
            if (!_roomManager.IsInRoom || IsWorking)
                return;

            _cts = new CancellationTokenSource();

            try
            {
                IsWorking = true;

                Clear();

                await SendAsync(Out.GetBannedUsers, (LegacyLong)_roomManager.CurrentRoomId);
                var packet = await Interceptor.ReceiveAsync(4000, In.UsersBannedFromRoom);

                long roomId = packet.ReadLegacyLong();
                if (roomId != _roomManager.CurrentRoomId)
                    throw new Exception($"Room ID mismatch (expected {_roomManager.CurrentRoomId}, received {roomId}).");

                short n = packet.ReadLegacyShort();
                for (int i = 0; i < n; i++)
                {
                    long userId = packet.ReadLegacyLong();
                    string userName = packet.ReadString();

                    var userViewModel = new BannedUserViewModel(userId, userName);
                    if (_userIdMap.TryAdd(userId, userViewModel))
                        AddUser(userViewModel);
                }
            }
            catch (OperationCanceledException)
            when (_cts.IsCancellationRequested)
            { }
            catch (Exception ex)
            { }
            finally
            {
                _cts.Dispose();
                _cts = null;

                IsWorking = false;
                IsCancelling = false;
            }
        }

        private async Task UnbanAsync(IEnumerable<BannedUserViewModel> users)
        {
            if (!_roomManager.IsInRoom || IsWorking)
                return;

            _cts = new CancellationTokenSource();

            try
            {
                long roomId = _roomManager.CurrentRoomId;
                if (roomId <= 0) return;

                var array = users.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    var user = array[i];
                    StatusText = $"Unbanning '{user.Name}'... ({i+1}/{array.Length})";
                    await SendAsync(Out.RoomUnbanUser, (LegacyLong)user.Id, (LegacyLong)roomId);
                    await Task.Delay(333, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            when (_cts.IsCancellationRequested)
            { }
            catch (Exception ex)
            {
                Dialog.ShowError(ex.Message);
            }
            finally
            {
                _cts.Dispose();
                _cts = null;

                IsWorking = false;
                IsUnbanning = false;
                IsCancelling = false;
                StatusText = string.Empty;
            }
        }
        #endregion
    }
}
