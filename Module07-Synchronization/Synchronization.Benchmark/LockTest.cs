using BenchmarkDotNet.Attributes;
using System;
using System.Threading;

namespace Synchronization.Benchmark
{
    [DisassemblyDiagnoser(exportCombinedDisassemblyReport: true)]
    [SimpleJob(warmupCount: 10, targetCount: 20)]
    public class LockTest
    {
        private const int QueueCapacity = 100;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark(Baseline = true)]
        public void Lock()
        {
            StartBench(new LockQueue<int>(QueueCapacity));
        }

        [Benchmark]
        public void ReaderWriterLock()
        {
            StartBench(new ReaderWriterLockQueue<int>(QueueCapacity));
        }

        [Benchmark]
        public void ReaderWriterLockSlim()
        {
            StartBench(new ReaderWriterLockSlimQueue<int>(QueueCapacity));
        }

        private void StartBench(IQueue<int> queue)
        {
            // Enqueue initial value for Peek() method to work
            queue.Enqueue(1);

            // We can use as many readers as processor cores available except for one doing writer work
            int readerCount = Environment.ProcessorCount - 1;

            // Event to signal start ot the test
            var startEvent = new ManualResetEventSlim(false);

            // Countdown event which signals when all reader threads have finished their work
            var readersFinished = new CountdownEvent(readerCount);

            // Preparing reader threads. They simply peek top item from the queue and signal
            // countdown event as soon as they finish.
            for (int threadIndex = 0; threadIndex < readerCount; threadIndex++)
            {
                var reader = new Thread(() =>
                {
                    startEvent.Wait();
                    for (int i = 0; i < 1_000_000; i++)
                        queue.Peek();

                    readersFinished.Signal();
                });
                reader.Start();
            }

            // Writer enqueues and dequeues items to the queue and stops as soon as all readers finished their job
            var writer = new Thread(() =>
            {
                startEvent.Wait();

                while (!readersFinished.IsSet)
                {
                    for (int i = 0; !readersFinished.IsSet || i < 100; i++)
                        queue.Enqueue(i);

                    for (int i = 0; !readersFinished.IsSet || i < 100; i++)
                        queue.Dequeue();
                }
            });
            writer.Start();

            // Signal all threads to start
            startEvent.Set();

            // Wait until all readers finish their job
            readersFinished.Wait();
        }
    }
}
