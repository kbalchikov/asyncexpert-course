using System;
using System.Threading;

namespace Synchronization.Core
{
    /*
     * Implement very simple wrapper around Mutex or Semaphore (remember both implement WaitHandle) to
     * provide a exclusive region created by `using` clause.
     *
     * Created region may be system-wide or not, depending on the constructor parameter.
     *
     * Any try to get a second systemwide scope should throw an `System.InvalidOperationException` with `Unable to get a global lock {name}.`
     */
    public class NamedExclusiveScope : IDisposable
    {
        private readonly Mutex _mutex;

        public NamedExclusiveScope(string name, bool isSystemWide)
        {
            string prefix = isSystemWide ? "Global\\" : "Local\\";
            string fullName = prefix + name;

            if (Mutex.TryOpenExisting(fullName, out Mutex _))
                throw new InvalidOperationException($"Unable to get a global lock {name}.");

            _mutex = new Mutex(true, fullName);
        }

        public void Dispose()
        {
            _mutex.Dispose();
        }
    }
}
