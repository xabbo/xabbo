using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using Xabbo.Interceptor;

namespace b7.Xabbo.ViewModel
{
    public class NavigatorViewManager : ComponentViewModel
    {
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public ICommand Search { get; set; }

        public NavigatorViewManager(IInterceptor interceptor)
            : base(interceptor)
        {
            Search = new RelayCommand(OnSearch);
        }

        private async void OnSearch()
        {

        }
    }
}
