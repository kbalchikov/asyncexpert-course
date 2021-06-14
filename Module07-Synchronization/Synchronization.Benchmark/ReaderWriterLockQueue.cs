using System;
using System.Collections.Generic;
using System.Threading;

namespace Synchronization.Benchmark
{
    public class ReaderWriterLockQueue<T> : IQueue<T>
    {
        private readonly ReaderWriterLock _lock = new();
        private readonly Queue<T> _queue;

        public ReaderWriterLockQueue(int capacity)
        {
            _queue = new Queue<T>(capacity);
        }

        public void Enqueue(T item)
        {
            _lock.AcquireWriterLock(TimeSpan.FromSeconds(1));
            try
            {
                _queue.Enqueue(item);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public T Dequeue()
        {
            _lock.AcquireWriterLock(TimeSpan.FromSeconds(1));
            try
            {
                return _queue.Dequeue();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public T Peek()
        {
            _lock.AcquireReaderLock(TimeSpan.FromSeconds(1));
            try
            {
                return _queue.Peek();
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }
    }
}
