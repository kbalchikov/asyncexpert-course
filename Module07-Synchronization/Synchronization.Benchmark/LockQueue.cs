using System.Collections.Generic;

namespace Synchronization.Benchmark
{
    public class LockQueue<T> : IQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly object _lock = new();

        public LockQueue(int capacity)
        {
            _queue = new Queue<T>(capacity);
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            lock (_lock)
            {
                return _queue.Dequeue();
            }
        }

        public T Peek()
        {
            lock (_lock)
            {
                return _queue.Peek();
            }
        }
    }
}
