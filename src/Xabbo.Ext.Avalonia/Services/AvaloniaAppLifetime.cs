using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Xabbo.Ext.Avalonia.Services;

internal class AvaloniaAppLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted => throw new NotImplementedException();

    public CancellationToken ApplicationStopping => throw new NotImplementedException();

    public CancellationToken ApplicationStopped => throw new NotImplementedException();

    public void StopApplication()
    {
        throw new NotImplementedException();
    }
}
