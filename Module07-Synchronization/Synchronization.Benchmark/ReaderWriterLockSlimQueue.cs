using System.Collections.Generic;
using System.Threading;

namespace Synchronization.Benchmark
{
    public class ReaderWriterLockSlimQueue<T> : IQueue<T>
    {
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly Queue<T> _queue;

        public ReaderWriterLockSlimQueue(int capacity)
        {
            _queue = new Queue<T>(capacity);
        }

        public void Enqueue(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _queue.Enqueue(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T Dequeue()
        {
            _lock.EnterWriteLock();
            try
            {
                return _queue.Dequeue();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T Peek()
        {
            _lock.EnterReadLock();
            try
            {
                return _queue.Peek();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
