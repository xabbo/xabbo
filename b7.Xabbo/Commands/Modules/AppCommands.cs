using System;
using System.Threading.Tasks;

using b7.Xabbo.Util;

namespace b7.Xabbo.Commands
{
    public class AppCommands : CommandModule
    {
        private readonly App _application;

        public AppCommands(App application)
        {
            _application = application;
        }

        [Command("x")]
        public Task OnShowWindow(CommandArgs args)
        {
            _application.Dispatcher.InvokeAsync(() => WindowUtil.Show(_application.MainWindow));

            return Task.CompletedTask;
        }
    }
}
