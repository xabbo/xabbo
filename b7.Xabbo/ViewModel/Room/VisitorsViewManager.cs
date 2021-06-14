using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

using b7.Xabbo.Services;
using Xabbo.Interceptor;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace b7.Xabbo.ViewModel
{
    public class VisitorsViewManager : ComponentViewModel
    {
#if PRO
        const string LOG_DIRECTORY = @"b7\xabbo\logs\visitors";
        string currentFilePath;
        DateTime lastDate;
#endif

        private readonly IUiContext context;
        private readonly ProfileManager profileManager;
        private readonly RoomManager roomManager;

        private readonly ConcurrentDictionary<long, VisitorViewModel> visitorMap = new();

        private readonly ObservableCollection<VisitorViewModel> visitors;

        private long lastRoomId = -1;

        public ICollectionView Visitors { get; }

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set => Set(ref _isAvailable, value);
        }

        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (Set(ref _filterText, value))
                    RefreshList();
            }
        }

        private bool isLogging;
        public bool IsLogging
        {
            get => isLogging;
            set => Set(ref isLogging, value);
        }

        public VisitorsViewManager(IInterceptor interceptor, IHostApplicationLifetime lifetime,
            IUiContext context,
            ProfileManager profileManager, RoomManager roomManager)
            : base(interceptor)
        {
            this.context = context;
            this.profileManager = profileManager;
            this.roomManager = roomManager;

            visitors = new ObservableCollection<VisitorViewModel>();

            Visitors = CollectionViewSource.GetDefaultView(visitors);
            Visitors.SortDescriptions.Add(new SortDescription("Entered", ListSortDirection.Descending));
            Visitors.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Descending));
            Visitors.Filter = Filter;

            lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
        }

        private bool Filter(object o)
        {
            if (string.IsNullOrWhiteSpace(_filterText))
                return true;

            if (o is not VisitorViewModel visitor) return false;

            return visitor.Name.ToLower().Contains(FilterText.ToLower());
        }

        public async Task InitializeAsync()
        {
            try
            {
                await profileManager.GetUserDataAsync();
            }
            catch { return; }

            roomManager.Left += Room_Left;
            roomManager.EntitiesAdded += Entities_EntitiesAdded;
            roomManager.EntityRemoved += Entities_EntityRemoved;

            IsAvailable = true;
        }

        private void RefreshList()
        {
            if (!context.IsSynchronized)
            {
                context.InvokeAsync(() => RefreshList());
                return;
            }

            Visitors.Refresh();
        }

        private void AddVisitors(IEnumerable<VisitorViewModel> newVisitors)
        {
            if (!context.IsSynchronized)
            {
                context.InvokeAsync(() => AddVisitors(newVisitors));
                return;
            }

            foreach (var visitor in newVisitors)
                visitors.Add(visitor);
        }

        private void ClearVisitors()
        {
            if (!context.IsSynchronized)
            {
                context.InvokeAsync(() => ClearVisitors());
                return;
            }

            visitors.Clear();
            visitorMap.Clear();
        }

#if PRO
        private void Log(string text)
        {
            if (!IsLogging || !roomManager.IsInRoom)
                return;

            DateTime today = DateTime.Today;
            if (today != lastDate || currentFilePath == null)
            {
                lastDate = today;
                currentFilePath = Path.Combine(LOG_DIRECTORY, $"{today:yyyy-MM-dd}.txt");
            }

            if (lastRoomId != roomManager.Id)
            {
                lastRoomId = roomManager.Id;

                string roomName = $"(roomid:{roomManager.Id})";
                if (roomManager.Data != null) roomName = $"{H.ReplaceSpecialCharacters(roomManager.Data.Name)} {roomName}";
                text = $"\r\n---------- {roomName} ----------\r\n\r\n" + text;
            }

            File.AppendAllText(currentFilePath, text, Encoding.UTF8);
        }
#endif

        private void Room_Left(object? sender, EventArgs e) => ClearVisitors();

        private void Entities_EntitiesAdded(object? sender, EntitiesEventArgs e)
        {
            if (!roomManager.IsLoadingRoom && !roomManager.IsInRoom)
                return;

            bool needsRefresh = false;
            var newLogs = new List<VisitorViewModel>();

            foreach (var user in e.Entities.OfType<IRoomUser>())
            {
                if (visitorMap.TryGetValue(user.Id, out VisitorViewModel? visitorLog))
                {
                    /* User already exists in the dictionary,
                     * so they have re-entered the room */
                    visitorLog.Visits++;
                    visitorLog.Index = user.Index;
                    visitorLog.Entered = DateTime.Now;
                    visitorLog.Left = null;
                    needsRefresh = true;

#if PRO
                    Log($"[{DateTime.UtcNow:O}]  In: {user.Name}\r\n");
#endif
                }
                else
                {
                    visitorLog = new VisitorViewModel(user.Index, user.Id, user.Name);
                    if (visitorMap.TryAdd(user.Id, visitorLog))
                    {
                        /* Entities received when first loading the room were already in the room,
                         * so we don't know when they entered, but we can see what order they
                         * entered the room by their entity index */
                        if (roomManager.IsLoadingRoom)
                        {
                            // Only set entry time for self
                            if (user.Id == profileManager.UserData?.Id)
                                visitorLog.Entered = DateTime.Now;
                        }
                        else
                        {
                            visitorLog.Entered = DateTime.Now;

#if PRO
                            Log($"[{DateTime.UtcNow:O}]  In: {user.Name}\r\n");
#endif
                        }

                        newLogs.Add(visitorLog);
                    }
                }
            }

            if (newLogs.Count > 0)
                context.InvokeAsync(() => newLogs.ForEach(x => visitors.Add(x)));
            /* The list gets refreshed when adding new items, only refresh the list 
             * if no new items were added and we need to re-order some items */
            if (newLogs.Count == 0 && needsRefresh)
                RefreshList();
        }


        private void Entities_EntityRemoved(object? sender, EntityEventArgs e)
        {
            if (visitorMap.TryGetValue(e.Entity.Id, out VisitorViewModel visitor))
            {
                visitor.Left = DateTime.Now;
#if PRO
                if (e.Entity.Id != profileManager.UserData?.Id)
                {
                    Log($"[{DateTime.UtcNow:O}] Out: {e.Entity.Name}\r\n");
                }
#endif
            }
        }
    }
}
