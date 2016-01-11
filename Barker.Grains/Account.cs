using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Barker.Interfaces;
using Orleans;
using Orleans.Streams;

namespace Barker.Grains
{
    public class AccountState : GrainState
    {
        public StreamSubscriptionHandle<Message> StreamSubscriptionHandle;
    }

    public class Account : Grain<AccountState>, IAccount
    {
        public const int MaxMessages = 25;
        public Guid PublicStreamId = Guid.Parse("{9246A005-4F23-401F-A8EA-DFEC8BEF5D96}");

        private Queue<Message> _receivedMessages;
        private IStreamProvider _streamProvider;
        private IAsyncStream<Message> _publicStream;
        private IAsyncStream<MessageBatch> _clientStream; 

        public override async Task OnActivateAsync()
        {
            _receivedMessages = new Queue<Message>();

            // acquire the message stream so that messages can be published
            _streamProvider = GetStreamProvider("DefaultProvider");
            _publicStream = _streamProvider.GetStream<Message>(PublicStreamId, "MessageStreams");

            if (null == State.StreamSubscriptionHandle)
            {
                // create the subscription and hook up a handler for receiving messages
                State.StreamSubscriptionHandle =
                    await _publicStream.SubscribeAsync((message, seq) => this.MessagePublished(message));
            }
            else
            {
                // the subscription already exists so just hook up a handler for receiving messages 
                State.StreamSubscriptionHandle = await State.StreamSubscriptionHandle.ResumeAsync((message, seq) => this.MessagePublished(message));
            }
            await this.WriteStateAsync();

            _clientStream = _streamProvider.GetStream<MessageBatch>(this.GetPrimaryKey(), "AccountSteams");

            // this timer is used to send received messages to the client
            this.RegisterTimer(UpdateSubscribers, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

            await base.OnActivateAsync();
        }

        public Task Publish(string text) => _publicStream.OnNextAsync(new Message(text, this.GetPrimaryKey()));

        public Task<Guid> AccountID() => Task.FromResult(this.GetPrimaryKey());

        private async Task UpdateSubscribers(object arg)
        {
            var messages = new MessageBatch(_receivedMessages.ToArray());

            var allSubscriptionHandles = await _clientStream.GetAllSubscriptionHandles();

            if (allSubscriptionHandles.Count == 0)
            {
                this.GetLogger().Verbose("no client subscribers");
            }

            await _clientStream.OnNextAsync(messages);
            _receivedMessages.Clear();
        }

        private Task MessagePublished(Message message)
        {
            if (null == message) return TaskDone.Done;
            if (message.AccountId == this.GetPrimaryKey()) return TaskDone.Done;

            GrainFactory.GetGrain<IHashtagPublisher>(this.RuntimeIdentity).ProcessMessage(message.Text);

            if (_receivedMessages.Count >= MaxMessages) _receivedMessages.Dequeue();
            _receivedMessages.Enqueue(message);
            return TaskDone.Done;
        }
    }
}