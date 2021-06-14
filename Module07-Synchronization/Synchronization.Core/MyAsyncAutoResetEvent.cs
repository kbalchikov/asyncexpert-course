using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Synchronization.Core
{
    public class MyAsyncAutoResetEvent
    {
        private readonly object _lock = new object();

        private readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue =
            new ConcurrentQueue<TaskCompletionSource<bool>>();

        private bool _signaled = true;

        public Task WaitAsync()
        {
            lock (_lock)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return Task.CompletedTask;
                }

                var tcs = new TaskCompletionSource<bool>();
                _queue.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Set()
        {
            lock (_lock)
            {
                if (_queue.TryDequeue(out TaskCompletionSource<bool> tcs))
                    tcs.TrySetResult(true);
                else
                    _signaled = true;
            }
        }

    }
}
