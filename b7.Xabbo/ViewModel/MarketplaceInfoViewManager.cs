using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabbo.Interceptor;

namespace b7.Xabbo.ViewModel
{
    public class MarketplaceInfoViewManager : ComponentViewModel
    {
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }




        public MarketplaceInfoViewManager(IInterceptor interceptor)
            : base(interceptor)
        { }


    }
}
