using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Dotnetos.AsyncExpert.Homework.Module01.Benchmark
{
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(exportCombinedDisassemblyReport: true)]
    public class FibonacciCalc
    {
        private Dictionary<ulong, ulong> _cache;

        // HOMEWORK:
        // 1. Write implementations for RecursiveWithMemoization and Iterative solutions
        // 2. Add MemoryDiagnoser to the benchmark
        // 3. Run with release configuration and compare results
        // 4. Open disassembler report and compare machine code
        // 
        // You can use the discussion panel to compare your results with other students

        [GlobalSetup]
        public void Setup()
        {
            _cache = new Dictionary<ulong, ulong>();
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(Data))]
        public ulong Recursive(ulong n)
        {
            if (n == 1 || n == 2) return 1;
            return Recursive(n - 2) + Recursive(n - 1);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public ulong RecursiveWithMemoization(ulong n)
        {
            if (n == 1 || n == 2)
                return 1;

            if (_cache.TryGetValue(n, out ulong value))
                return value;

            value = RecursiveWithMemoization(n - 2) + RecursiveWithMemoization(n - 1);
            _cache[n] = value;
            return value;
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public ulong Iterative(ulong n)
        {
            ulong prevPrevNumber, prevNumber = 0, currentNumber = 1;
            for (ulong i = 1; i < n; i++)
            {
                prevPrevNumber = prevNumber;
                prevNumber = currentNumber;
                currentNumber = prevNumber + prevPrevNumber;
            }
            return currentNumber;
        }

        public IEnumerable<ulong> Data()
        {
            yield return 15;
            yield return 35;
        }
    }
}
