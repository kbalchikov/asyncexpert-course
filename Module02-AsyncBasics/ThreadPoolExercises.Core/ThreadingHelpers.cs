using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolExercises.Core
{
    public class ThreadingHelpers
    {
        public static void ExecuteOnThread(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Create a thread and execute there `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `Join` to wait until created Thread finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            for (int i = 0; i < repeats; i++)
            {
                if (token.IsCancellationRequested)
                {
                    errorAction?.Invoke(new TaskCanceledException());
                    return;
                }

                Exception? threadException = null;
                var thread = new Thread(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception exc)
                    {
                        threadException = exc;
                    }
                });
                thread.Start();
                thread.Join();

                if (threadException != null)
                {
                    errorAction?.Invoke(threadException);
                    return;
                }
            }
        }

        public static void ExecuteOnThreadPool(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Queue work item to a thread pool that executes `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `AutoResetEvent` to wait until the queued work item finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            for (int i = 0; i < repeats; i++)
            {
                if (token.IsCancellationRequested)
                {
                    errorAction?.Invoke(new TaskCanceledException());
                    return;
                }

                var autoResetEvent = new AutoResetEvent(false);
                Exception? threadException = null;

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    autoResetEvent.Reset();
                    try
                    {
                        action();
                    }
                    catch (Exception exc)
                    {
                        threadException = exc;
                    }
                    finally
                    {
                        autoResetEvent.Set();
                    }
                });

                autoResetEvent.WaitOne();

                if (threadException != null)
                {
                    errorAction?.Invoke(threadException);
                    return;
                }
            }
        }
    }
}
