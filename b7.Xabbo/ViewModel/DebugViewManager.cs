using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace b7.Xabbo.ViewModel
{
    public class DebugViewManager
    {
        private readonly ISnackbarMessageQueue _snackbarMq;

        public ICommand TestSnackbar { get; }

        public DebugViewManager(ISnackbarMessageQueue snackbarMq)
        {
            _snackbarMq = snackbarMq;

            TestSnackbar = new RelayCommand(OnTestSnackbar);
        }

        private void OnTestSnackbar()
        {
            _snackbarMq.Enqueue("Hello, world");
        }
    }
}
