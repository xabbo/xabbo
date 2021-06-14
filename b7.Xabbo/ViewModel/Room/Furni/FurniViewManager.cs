using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using System.Diagnostics;

using GalaSoft.MvvmLight.Command;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Events;

using b7.Xabbo.Services;
using Xabbo.Interceptor;
using Microsoft.Extensions.Hosting;

namespace b7.Xabbo.ViewModel
{
    public class FurniViewManager : ComponentViewModel, INotifyDataErrorInfo
    {
        #region - Error handling -
        private readonly Dictionary<string, List<string>> errors = new();

        private void ClearErrors()
        {
            errors.Clear();
            RaiseErrorsChanged(null);
        }

        private void ClearErrors(string propertyName)
        {
            errors.Remove(propertyName);
            RaiseErrorsChanged(propertyName);
        }

        private void AddError(string propertyName, string errorText)
        {
            if (!errors.ContainsKey(propertyName))
                errors[propertyName] = new List<string>();
            errors[propertyName].Add(errorText);
            RaiseErrorsChanged(propertyName);
        }

        public bool HasErrors => errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        protected void RaiseErrorsChanged(string? propertyName)
            => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName is not null &&
                errors.ContainsKey(propertyName))
            {
                return errors[propertyName];
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }
        #endregion

        private readonly IUiContext _uiContext;
        private readonly ProfileManager _profileManager;
        private RoomManager _roomManager;

        private readonly IGameDataManager _gameDataManager;
        private FurniData? FurniData => _gameDataManager.FurniData;
        private ExternalTexts? Texts => _gameDataManager.ExternalTexts;

        private readonly ObservableCollection<FurniViewModel> _furniViewModels = new();
        private readonly ConcurrentDictionary<long, FurniViewModel>
            _floorItemMap = new(),
            _wallItemMap = new();

        private CancellationTokenSource? _cts;
        private bool _isPickingUp;

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
                    FilterTextUpdated();
            }
        }

        private bool _isQuery;
        public bool IsQuery
        {
            get => _isQuery;
            set
            {
                if (Set(ref _isQuery, value))
                    UpdateFilter();
            }
        }

        private bool _filterNeedsUpdate;
        public bool FilterNeedsUpdate
        {
            get => _filterNeedsUpdate;
            set => Set(ref _filterNeedsUpdate, value);
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }

        private IList<FurniViewModel> _selectedItems = Array.Empty<FurniViewModel>();
        public IList<FurniViewModel> SelectedItems
        {
            get => _selectedItems;
            set => Set(ref _selectedItems, value);
        }

        public bool CanShow => !IsWorking && SelectedItems.Any(x => x.IsHidden);
        public bool CanHide => !IsWorking && SelectedItems.Any(x => !x.IsHidden);
        public bool CanPickUp => !IsWorking && SelectedItems.Any(x => x.OwnerId == _profileManager.UserData?.Id);
        public bool CanEject => !IsWorking &&
            _roomManager.RightsLevel >= 3 &&
            SelectedItems.Any(x => x.OwnerId != _profileManager.UserData?.Id);

        public ICommand RefreshFilterCommand { get; }

        public ICommand ShowCommand { get; }
        public ICommand HideCommand { get; }
        public ICommand PickupCommand { get; }
        public ICommand EjectCommand { get; }
        public ICommand SetStateCommand { get; }
        public ICommand RotateCommand { get; }

        public ICollectionView Furni { get; }

        private void FilterTextUpdated()
        {
            if (IsQuery)
            {
                FilterNeedsUpdate = true;
            }
            else
            {
                UpdateFilter();
            }
        }

        private bool UpdateFilter()
        {
            if (FilterText == null)
                FilterText = string.Empty;

            if (IsQuery)
            {
                string query = FilterText.Trim();

                ClearErrors(nameof(FilterText));

                try
                {
                    if (!string.IsNullOrWhiteSpace(query))
                    {

                        var lambdaExpression = DynamicExpressionParser.ParseLambda(
                            typeof(FurniViewModel),
                            typeof(bool),
                            FilterText
                        );

                        dynamicFilter = (Func<FurniViewModel, bool>)lambdaExpression.Compile();
                    }
                    else
                    {
                        dynamicFilter = x => true;
                    }
                }
                catch (Exception ex)
                {
                    AddError(nameof(FilterText), ex.Message);
                    return false;
                }

                UpdateViewModels();
                RefreshList();
            }
            else
            {
                ClearErrors(nameof(FilterText));

                string s = FilterText.ToLower();
                dynamicFilter = x => x.Name.ToLower().Contains(s);

                RefreshList();
            }

            FilterNeedsUpdate = false;
            return true;
        }

        private Func<FurniViewModel, bool>? dynamicFilter;

        private bool FilterFurni(object o)
        {
            if (o is not FurniViewModel furniViewModel) return false;

            return dynamicFilter?.Invoke(furniViewModel) ?? true;
        }

        public FurniViewManager(IHostApplicationLifetime lifetime,
            IInterceptor interceptor,
            IUiContext uiContext,
            IGameDataManager gameDataManager,
            ProfileManager profileManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _uiContext = uiContext;
            _gameDataManager = gameDataManager;
            _profileManager = profileManager;
            _roomManager = roomManager;

            Furni = CollectionViewSource.GetDefaultView(_furniViewModels);
            Furni.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Furni.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            Furni.Filter = FilterFurni;

            RefreshFilterCommand = new RelayCommand(OnRefreshFilter, CanRefreshFilter);

            ShowCommand = new RelayCommand<IList>(OnShow);
            HideCommand = new RelayCommand<IList>(OnHide);
            PickupCommand = new RelayCommand<IList>(OnPickup);
            EjectCommand = new RelayCommand<IList>(OnEject);

            _roomManager.FurniVisibilityToggled += RoomManager_FurniVisibilityToggled;

            lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
        }

        public void RefreshCommandsCanExecute()
        {
            RaisePropertyChanged(nameof(CanShow));
            RaisePropertyChanged(nameof(CanHide));
            RaisePropertyChanged(nameof(CanPickUp));
            RaisePropertyChanged(nameof(CanEject));
        }

        private void RoomManager_FurniVisibilityToggled(object? sender, FurniEventArgs e)
        {
            var map = (e.Item.Type == ItemType.Floor) ? _floorItemMap : _wallItemMap;
            if (map.TryGetValue(e.Item.Id, out FurniViewModel? viewModel))
                viewModel.IsHidden = e.Item.IsHidden;
        }

        private void UpdateViewModels()
        {
            lock (_furniViewModels)
            {
                foreach (var furniViewModel in _furniViewModels)
                {
                    IFurni? furni = _roomManager.Room?.GetFurni(furniViewModel.Type, furniViewModel.Id);
                    if (furni is null) continue;
                    furniViewModel.Update(furni);
                }
            }
        }

        private bool CanRefreshFilter() => true;

        private void OnRefreshFilter()
        {
            UpdateFilter();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await Task.WhenAll(
                    _profileManager.GetUserDataAsync(),
                    _gameDataManager.GetFurniDataAsync(),
                    _gameDataManager.GetExternalTextsAsync()
                );
            }
            catch
            {
                return;
            }

            _roomManager.RightsUpdated += RoomManager_RightsUpdated;
            _roomManager.Left += RoomManager_Left;

            _roomManager.FloorItemsLoaded += FurniManager_FloorItemsLoaded;
            _roomManager.FloorItemAdded += FurniManager_FloorItemAdded;
            _roomManager.FloorItemRemoved += FurniManager_FloorItemRemoved;

            _roomManager.WallItemsLoaded += FurniManager_WallItemsLoaded;
            _roomManager.WallItemAdded += FurniManager_WallItemAdded;
            _roomManager.WallItemRemoved += FurniManager_WallItemRemoved;

            IsAvailable = true;
        }

        private void RoomManager_RightsUpdated(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanEject));
        }

        private void RefreshList()
        {
            if (!_uiContext.IsSynchronized)
            {
                _uiContext.InvokeAsync(() => RefreshList());
                return;
            }

            Furni.Refresh();
        }

        private void ClearItems()
        {
            if (!_uiContext.IsSynchronized)
            {
                _uiContext.InvokeAsync(() => ClearItems());
                return;
            }

            lock (_furniViewModels) _furniViewModels.Clear();

            _floorItemMap.Clear();
            _wallItemMap.Clear();
        }

        private void AddItem(FurniViewModel furni)
        {
            if (!_uiContext.IsSynchronized)
            {
                _uiContext.InvokeAsync(() => AddItem(furni));
                return;
            }

            lock (_furniViewModels) _furniViewModels.Add(furni);

            var map = furni.Type == ItemType.Floor ? _floorItemMap : _wallItemMap;
            if (!map.TryAdd(furni.Id, furni))
                Debug.WriteLine($"[RoomFurniViewManager] AddItem failed to add item {furni.Id} to dictionary");
        }

        private void AddItems(IEnumerable<FurniViewModel> furnis)
        {
            if (!_uiContext.IsSynchronized)
            {
                _uiContext.InvokeAsync(() => AddItems(furnis));
                return;
            }

            foreach (var furni in furnis)
                AddItem(furni);
        }

        private void RemoveItem(ItemType type, long id)
        {
            if (!_uiContext.IsSynchronized)
            {
                _uiContext.InvokeAsync(() => RemoveItem(type, id));
                return;
            }

            var map = type == ItemType.Floor ? _floorItemMap : _wallItemMap;
            if (!map.TryRemove(id, out FurniViewModel? furniViewModel))
            {
                Debug.WriteLine($"[RoomFurniViewManager] RemoveItem failed to remove item {id} from dictionary");
                return;
            }

            lock (_furniViewModels) _furniViewModels.Remove(furniViewModel);
        }

        private FurniViewModel WrapItem(IFurni item)
        {
            if (FurniData is null ||
                Texts is null)
            {
                throw new InvalidOperationException("Game data is not initialized.");
            }

            FurniInfo info = FurniData.GetInfo(item);

            string
                name = info.Name,
                description = info.Description;

            if (info.Identifier == "poster" && item is IWallItem wallItem)
            {
                Texts?.TryGetValue($"poster_{wallItem.Data}_name", out name);
                Texts?.TryGetValue($"poster_{wallItem.Data}_desc", out description);
            }

            if (string.IsNullOrWhiteSpace(name)) name = info.Identifier;
            if (string.IsNullOrWhiteSpace(description)) description = string.Empty;

            return new FurniViewModel(info, item, name, description);
        }

        #region - Commands -
        private void OnShow(IList? selected)
        {
            if (selected is null) return;

            foreach (var viewModel in selected.OfType<FurniViewModel>())
            {
                viewModel.IsHidden = false;
                _roomManager.ShowFurni(viewModel.Type, viewModel.Id);
            }
        }

        private void OnHide(IList? selected)
        {
            if (selected is null) return;

            foreach (var viewModel in selected.OfType<FurniViewModel>())
            {
                viewModel.IsHidden = true;
                _roomManager.HideFurni(viewModel.Type, viewModel.Id);
            }
        }

        private async void OnPickup(IList? selected)
        {
            if (!IsAvailable || IsWorking || selected == null)
                return;

            IsWorking = true;
            try
            {
                var items = selected
                    .OfType<FurniViewModel>()
                    .Where(x => x.OwnerId == _profileManager.UserData?.Id)
                    .ToArray();

                foreach (var item in items)
                {
                    _roomManager.Pickup(item.Type, item.Id);
                    await Task.Delay(150);
                }
            }
            catch { }
            finally
            {
                IsWorking = false;
            }
        }

        private async void OnEject(IList? selected)
        {
            if (!IsAvailable || IsWorking || selected == null)
                return;

            IsWorking = true;
            try
            {
                var items = selected
                    .OfType<FurniViewModel>()
                    .Where(x => x.OwnerId != _profileManager.UserData?.Id)
                    .ToArray();

                if (items.Length == 0) return;

                if (!Dialog.ConfirmYesNoWarning($"Are you sure you want to eject {items.Length} furni from the room?"))
                    return;

                foreach (var item in items)
                {
                    _roomManager.Pickup(item.Type, item.Id);
                    await Task.Delay(150);
                }
            }
            catch { }
            finally
            {
                IsWorking = false;
            }
        }
        #endregion

        #region - Events -
        private void FurniManager_FloorItemsLoaded(object? sender, FloorItemsEventArgs e)
        {
            AddItems(e.Items.Select(item => WrapItem(item)));
        }

        private void FurniManager_FloorItemAdded(object? sender, FloorItemEventArgs e)
        {
            AddItem(WrapItem(e.Item));
        }

        private void FurniManager_FloorItemRemoved(object? sender, FloorItemEventArgs e)
        {
            RemoveItem(ItemType.Floor, e.Item.Id);
        }

        private void FurniManager_WallItemsLoaded(object? sender, WallItemsEventArgs e)
        {
            AddItems(e.Items.Select(item => WrapItem(item)));
        }

        private void FurniManager_WallItemAdded(object? sender, WallItemEventArgs e)
        {
            var view = WrapItem(e.Item);
            AddItem(view);
        }

        private void FurniManager_WallItemRemoved(object? sender, WallItemEventArgs e)
        {
            RemoveItem(ItemType.Wall, e.Item.Id);
        }

        private void RoomManager_Left(object? sender, EventArgs e)
        {
            ClearItems();
        }
        #endregion
    }
}
