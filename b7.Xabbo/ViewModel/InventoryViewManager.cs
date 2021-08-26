using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xabbo.Core;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.ViewModel
{
    public class InventoryViewManager : ComponentViewModel
    {
        private readonly List<InventoryItem> _loadList = new();
        private int _currentFragment, _totalFragments;

        private bool _flattenInventory;
        public bool FlattenInventory
        {
            get => _flattenInventory;
            set => Set(ref _flattenInventory, value);
        }

        public InventoryViewManager(IInterceptor interceptor)
            : base(interceptor)
        { }

        [InterceptIn(nameof(Incoming.InventoryPush))]
        protected virtual void HandleInventoryPush(InterceptArgs e)
        {
            if (_flattenInventory) e.Block();

            InventoryFragment fragment = InventoryFragment.Parse(e.Packet);
            if ((fragment.Index == 0 && _currentFragment > 0) || fragment.Total != _totalFragments)
            {
                _loadList.Clear();
                _totalFragments = fragment.Total;
                _currentFragment = fragment.Index;
            }

            _loadList.AddRange(fragment);
            _currentFragment++;

            if (_currentFragment == _totalFragments)
            {
                if (_flattenInventory)
                {
                    foreach (var item in _loadList)
                    {
                        
                    }

                    List<InventoryFragment> fragments = new();
                    int n = 0;
                    foreach (var group in _loadList.GroupBy(x => n++ / 600))
                    {
                        fragments.Add(new InventoryFragment(group));
                    }

                    for (int i = 0; i < fragments.Count; i++)
                    {
                        fragments[i].Index = i;
                        fragments[i].Total = fragments.Count;
                    }
                }
            }
        }
    }
}
