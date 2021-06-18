using BenchmarkDotNet.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace DataStructures.Benchmark
{
    public class ConcurrentStructuresTest
    {
        private const int KeyCount = 16;
        private const int ValueCount = 100000;
        private const int ConcurrentWriters = 2;

        [Benchmark(Baseline = true)]
        public async Task LockingMetricsCounter()
        {
            await Test<LockingMetricsCounter>();
        }

        [Benchmark]
        public async Task ConcurrentDictionaryOnlyMetricsCounter()
        {
            await Test<ConcurrentDictionaryOnlyMetricsCounter>();
        }

        [Benchmark]
        public async Task ConcurrentDictionaryWithCounterMetricsCounter()
        {
            await Test<ConcurrentDictionaryWithCounterMetricsCounter>();
        }

        private async Task Test<TMetricCounter>() where TMetricCounter : IMetricsCounter, new()
        {
            var originalKeys = Enumerable.Range(0, KeyCount).Select(i => i.ToString()).ToArray();
            var keys = Enumerable.Repeat(originalKeys, ConcurrentWriters).SelectMany(m => m).ToArray();

            var starter = new TaskCompletionSource<object>();
            var counter = new TMetricCounter();

            // run two tasks per key
            var tasks = keys.Select(key => Task.Run(async () =>
            {
                await starter.Task;
                for (var i = 0; i < ValueCount; i++)
                {
                    counter.Increment(key);
                }
            })).ToArray();

            // start it
            starter.SetResult(starter);
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
