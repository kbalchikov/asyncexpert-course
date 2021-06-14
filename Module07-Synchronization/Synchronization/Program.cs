using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synchronization.Core;

namespace Synchronization
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var are = new MyAsyncAutoResetEvent();
            var random = new Random();

            var tasks = Enumerable.Range(1, 8).Select(_ =>
            {
                return Task.Run(async () =>
                {
                    int number = random.Next(100, 1000);
                    Console.WriteLine($"Task #{number} waiting to start");
                    await are.WaitAsync();

                    Console.WriteLine($"Task #{number} doing hard work");
                    await Task.Delay(1000);
                    Console.WriteLine($"Task #{number} completed");

                    are.Set();
                });
            });

            await Task.WhenAll(tasks);
            Console.WriteLine("All completed");
        }
    }
}
