using System;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;

namespace b7.Xabbo.ViewModel;

public class GiftViewModel : ObservableObject
{

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

    private BitmapImage _itemImage;
    public BitmapImage ItemImage
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

    private BitmapImage _senderImage;
    public BitmapImage SenderImage
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
