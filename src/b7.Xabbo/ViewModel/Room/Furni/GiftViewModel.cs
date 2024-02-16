using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Xabbo.Core;

namespace b7.Xabbo.ViewModel;

public class GiftViewModel : ObservableObject
{
    public IFloorItem Item { get; init; }

    private string _itemName;
    public string ItemName
    {
        get => _itemName;
        set => SetProperty(ref _itemName, value);
    }

    private string _itemIdentifier;
    public string ItemIdentifier
    {
        get => _itemIdentifier;
        set => SetProperty(ref _itemIdentifier, value);
    }

    private Uri _itemImage;
    public Uri ItemImageUri
    {
        get => _itemImage;
        set => SetProperty(ref _itemImage, value);
    }

    private string _message;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private string _senderName;
    public string SenderName
    {
        get => _senderName;
        set => SetProperty(ref _senderName, value);
    }

    private Uri _senderImage;
    public Uri SenderImageUri
    {
        get => _senderImage;
        set => SetProperty(ref _senderImage, value);
    }

    private string _extraParameter;
    public string ExtraParameter
    {
        get => _extraParameter;
        set => SetProperty(ref _extraParameter, value);
    }
}
