using System.Collections.Generic;
using System.Threading;

namespace Synchronization.Core
{
    public class MyStringInternPool
    {
        private readonly HashSet<string> _cachedStrings = new HashSet<string>();
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public void Clear()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _cachedStrings.Clear();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        // Intern with two locks - read, and write
        public string Intern(string str)
        {
            string val;

            _rwLock.EnterReadLock();
            try
            {
                if (_cachedStrings.TryGetValue(str, out val))
                    return val;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            _rwLock.EnterWriteLock();
            try
            {
                if (_cachedStrings.TryGetValue(str, out val))
                    return val;

                _cachedStrings.Add(str);
                return str;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        // Intern with upgradeable lock
        public string UpgradableIntern(string str)
        {
            string val;

            _rwLock.EnterUpgradeableReadLock();
            try
            {
                if (_cachedStrings.TryGetValue(str, out val))
                    return val;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_cachedStrings.TryGetValue(str, out val))
                        return val;

                    _cachedStrings.Add(str);
                    return str;
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }
        }

        public bool TryGetInterned(string str, out string interned)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_cachedStrings.TryGetValue(str, out interned))
                    return true;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }
    }
}
