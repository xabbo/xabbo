using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.LogicalTree;

using Xabbo.ViewModels;

namespace Xabbo.Avalonia.Views;

public partial class RoomAvatarsView : UserControl
{
    private DataGridCollectionView? _collectionView;

    private RoomAvatarsViewModel? _viewModel;

    public RoomAvatarsView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        AvatarDataGrid.Sorting += OnSorting;
        DetachedFromLogicalTree += OnDetached;
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (DataContext is not RoomAvatarsViewModel avatarsViewModel)
            return;

        avatarsViewModel.ContextSelection = AvatarDataGrid
            .SelectedItems
            .OfType<AvatarViewModel>()
            .ToList();
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AvatarDataGrid.ItemsSource = _collectionView;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AvatarDataGrid.ItemsSource = null;
    }

    private void OnDetached(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RefreshList -= OnRefreshList;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            return;

        _viewModel = DataContext as RoomAvatarsViewModel;
        if (_viewModel is null)
            return;

        _collectionView = new DataGridCollectionView(_viewModel.Avatars, false, false);
        _collectionView.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(AvatarViewModel.Group)));
        _collectionView.SortDescriptions.Add(new DataGridComparerSortDescription(
            AvatarViewModelGroupComparer.Default,
            ListSortDirection.Ascending
        ));
        _viewModel.RefreshList += OnRefreshList;

        AvatarDataGrid.ItemsSource = _collectionView;
    }

    private void OnSorting(object? sender, DataGridColumnEventArgs e)
    {
        if (_collectionView is null) return;

        var sortDescription = _collectionView
            .SortDescriptions
            .FirstOrDefault(x => x.PropertyPath == e.Column.SortMemberPath);

        _collectionView.SortDescriptions.Clear();
        _collectionView.SortDescriptions.Add(
            new DataGridComparerSortDescription(AvatarViewModelGroupComparer.Default, ListSortDirection.Ascending));

        if (sortDescription is not null)
        {
            _collectionView.SortDescriptions.Add(sortDescription.SwitchSortDirection());
        }
        else
        {
            _collectionView.SortDescriptions.Add(DataGridSortDescription.FromPath(
                e.Column.SortMemberPath,
                ListSortDirection.Ascending,
                e.Column.SortMemberPath switch
                {
                    "Index" => Comparer<int>.Default,
                    "Id" => Comparer<long>.Default,
                    "Name" => StringComparer.OrdinalIgnoreCase,
                    _ => throw new Exception($"Unknown sort member: '{e.Column.SortMemberPath}'.")
                }
            ));
        }
        e.Handled = true;
    }

    private void OnRefreshList()
    {
        _collectionView?.Refresh();
    }
}
