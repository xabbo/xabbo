using System;
using System.Windows.Media.Imaging;

using GalaSoft.MvvmLight;

namespace b7.Xabbo.ViewModel
{
    public class GiftViewModel : ObservableObject
    {

        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set => Set(ref _itemName, value);
        }

        private string _itemIdentifier;
        public string ItemIdentifier
        {
            get => _itemIdentifier;
            set => Set(ref _itemIdentifier, value);
        }

        private BitmapImage _itemImage;
        public BitmapImage ItemImage
        {
            get => _itemImage;
            set => Set(ref _itemImage, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private string _senderName;
        public string SenderName
        {
            get => _senderName;
            set => Set(ref _senderName, value);
        }

        private BitmapImage _senderImage;
        public BitmapImage SenderImage
        {
            get => _senderImage;
            set => Set(ref _senderImage, value);
        }


        private string _extraParameter;
        public string ExtraParameter
        {
            get => _extraParameter;
            set => Set(ref _extraParameter, value);
        }


    }
}
