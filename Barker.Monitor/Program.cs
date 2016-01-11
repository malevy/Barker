using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Barker.Interfaces;
using Orleans;

namespace Barker.Monitor
{
    class Program
    {
        private static readonly string ClientName = Guid.NewGuid().ToString();

        static void Main(string[] args)
        {
            Console.WriteLine($"Starting monitor {ClientName}");

            GrainClient.Initialize("ClientConfiguration.xml");

            var cts = new CancellationTokenSource();
            var workTask = Task.Factory.StartNew(DoWork, cts.Token);

            Console.WriteLine("Monitor started");
            Console.WriteLine("Press ENTER to stop this monitor.");
            Console.ReadLine();

            Console.WriteLine("Stopping monitor");
            cts.Cancel();
            GrainClient.Uninitialize();
            workTask = null;
            Console.WriteLine("Monitor stopped");
        }

        private static async void DoWork()
        {

            var trendingHashtags = GrainClient.GrainFactory.GetGrain<ITrendingHashtags>(0);

            while (true)
            {
                Console.WriteLine($"Trending now:");
                var trends = await trendingHashtags.GetTrending();
                trends
                    .OrderByDescending(t => t.Value)
                    .Take(4)
                    .ToList()
                    .ForEach(t => Console.WriteLine($"{t.Key} ({t.Value})"));

                Console.WriteLine();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

        }
    }
}
