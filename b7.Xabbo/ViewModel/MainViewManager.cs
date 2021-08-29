using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MaterialDesignThemes.Wpf;

using GalaSoft.MvvmLight;

using Xabbo.Interceptor;

namespace b7.Xabbo.ViewModel
{
    public class MainViewManager : ObservableObject
    {
        private readonly IRemoteInterceptor _interceptor;
        private Task? _interceptorTask;

        private string _title = "xabbo";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        public GeneralViewManager General { get; }
        // Chat
        public ChatLogViewManager Chat { get; }
        // Figure
        public WardrobeViewManager Wardrobe { get; }
        public FigureRandomizerViewManager FigureRandomizer { get; }
        // Room
        public RoomInfoViewManager RoomInfo { get; }
        public EntitiesViewManager Entities { get; }
        public VisitorsViewManager Visitors { get; }
        public BanListViewManager BanList { get; }
        public FurniViewManager Furni { get; }
        // Tools
        public AlignerViewManager Aligner { get; }
        public MimicViewManager Mimic { get; }
        // Info
        public FurniDataViewManager FurniData { get; }

        public MainViewManager(ISnackbarMessageQueue snackbarMessageQueue,
            IRemoteInterceptor interceptor,
            GeneralViewManager general,
            ChatLogViewManager chat,
            WardrobeViewManager wardrobe,
            FigureRandomizerViewManager figureRandomizer,
            RoomInfoViewManager roomInfo,
            EntitiesViewManager entities,
            VisitorsViewManager visitors,
            BanListViewManager banList,
            FurniViewManager furni,
            AlignerViewManager aligner,
            MimicViewManager mimic,
            FurniDataViewManager furniData)
        {
            SnackbarMessageQueue = snackbarMessageQueue;

            _interceptor = interceptor;

            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version is not null)
            {
                Title = $"xabbo v{version.ToString(3)}";
            }

            General = general;
            Chat = chat;
            Wardrobe = wardrobe;
            FigureRandomizer = figureRandomizer;
            RoomInfo = roomInfo;
            Entities = entities;
            Visitors = visitors;
            BanList = banList;
            Furni = furni;
            Aligner = aligner;
            Mimic = mimic;
            FurniData = furniData;
        }

        public Task InitializeAsync()
        {
            _interceptor.Initialized += OnInitialized;
            _interceptor.Connected += OnConnected;

            _interceptorTask = RunInterceptorAsync();

            return Task.CompletedTask;
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
        }

        private void OnConnected(object? sender, GameConnectedEventArgs e)
        {

        }

        private async Task RunInterceptorAsync()
        {
            await _interceptor.RunAsync();
        }
    }
}
