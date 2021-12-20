using System;
using System.Threading;
using System.Threading.Tasks;

namespace b7.Xabbo.Services
{
    public class OperationManager : IOperationManager
    {
        private readonly object _sync = new();
        private CancellationTokenSource? _cts;

        public OperationManager() { }

        public bool IsTaskRunning => _cts is not null;

        public void CancelCurrentTask()
        {
            _cts?.Cancel();
        }

        public async Task RunTaskAsync(Func<CancellationToken, Task> task)
        {
            lock (_sync)
            {
                if (_cts is not null)
                {
                    throw new InvalidOperationException("A task is already running.");
                }

                _cts = new CancellationTokenSource();
            }

            try
            {
                await task(_cts.Token);
            }
            catch (OperationCanceledException)
            when (_cts.IsCancellationRequested)
            { }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
