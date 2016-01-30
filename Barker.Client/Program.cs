using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Barker.Interfaces;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Polly;

namespace Barker.Client
{
    class Program
    {
        private static readonly Guid ClientName = Guid.NewGuid();

        static void Main(string[] args)
        {
            Console.WriteLine($"Starting client {ClientName}");

            var configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "ClientConfiguration.xml");
            GrainClient.Initialize(configFile);

            var streamProvider = GrainClient.GetStreamProvider("DefaultProvider");

            var messageStream = streamProvider.GetStream<MessageBatch>(ClientName, "AccountSteams");

            var watcher = new MessageWatcher();
            var subscriptionHandle = messageStream.SubscribeAsync(watcher).Result;

            var cts = new CancellationTokenSource();
            var publisherTask = Task.Factory.StartNew(PublishMessages, cts.Token);

            Console.WriteLine("client started");
            Console.WriteLine("Press ENTER to stop this publisher.");
            Console.ReadLine();
            
            Console.WriteLine("Stopping client");
            cts.Cancel();
            publisherTask = null;
            subscriptionHandle.UnsubscribeAsync().Wait();

            GrainClient.Uninitialize();
            Console.WriteLine("Publisher client");
        }

        private static void PublishMessages()
        {
            var account = GrainClient.GrainFactory.GetGrain<IAccount>(ClientName);

            string[] hashtags = new[]
            {
                "tickle", "pack", "dizzy", "possessive", "mountain", "striped",
                "Unequaled", "care", "trashy", "serve", "bone", "fat"
            };

            var retryPolicy = Policy
                .Handle<AggregateException>()
                .WaitAndRetry(4, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            while (true)
            {
                var message = $"{LoremNET.Lorem.Words(2, 10)} #{LoremNET.Lorem.Random(hashtags)}";
                Console.WriteLine($">> {message}");
                retryPolicy.Execute(() => account.Publish(message).Wait());
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

        }
    }

    class MessageWatcher : IAsyncObserver<MessageBatch>
    {
        public Task OnNextAsync(MessageBatch item, StreamSequenceToken token = null)
        {
            if (null == item) return TaskDone.Done;
            Console.WriteLine("Received {0} messages", (item.Messages?.Length ?? 0));
            if (!(item.Messages?.Any() ?? false)) return TaskDone.Done;

            item.Messages.ToList()
                .ForEach(
                    message =>
                        Console.WriteLine($"<< {message.Published.ToLocalTime()}\t{message.AccountId}\t{message.Text}")
                );

            return TaskDone.Done;
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return TaskDone.Done;
        }
    }
}