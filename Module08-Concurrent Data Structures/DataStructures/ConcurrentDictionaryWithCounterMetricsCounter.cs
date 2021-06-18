using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace DataStructures
{
    public class ConcurrentDictionaryWithCounterMetricsCounter : IMetricsCounter
    {
        // Implement this class using ConcurrentDictionary and the provided AtomicCounter class.
        // AtomicCounter should be created only once per key, then its Increment method should be used.
        private readonly ConcurrentDictionary<string, AtomicCounter> dict = new ConcurrentDictionary<string, AtomicCounter>();

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return dict.Select(x => new KeyValuePair<string, int>(x.Key, x.Value.Count)).GetEnumerator();
        }

        public void Increment(string key)
        {
            dict.GetOrAdd(key, _ => new AtomicCounter()).Increment();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class AtomicCounter
        {
            int value;

            public void Increment() => Interlocked.Increment(ref value);

            public int Count => Volatile.Read(ref value);
        }
    }
}