using System;
using System.Collections.Generic;
using System.Linq;
using Orleans.Concurrency;

namespace Barker.Interfaces
{
    /// <summary>
    /// A message being sent between Accounts
    /// </summary>
    [Serializable, Immutable]
    public class Message
    {
        public Message(string text, Guid accountId) : this(text, accountId, DateTime.UtcNow)
        {
        }

        public Message(string text, Guid accountId, DateTime published)
        {
            Text = text;
            AccountId = accountId;
            Published = published;
        }

        public string Text { get; }
        public Guid AccountId { get; }
        public DateTime Published { get; }
    }

    /// <summary>
    /// A batch of messages sent from an Account to a client
    /// </summary>
    [Serializable, Immutable]
    public class MessageBatch
    {
        public MessageBatch(IEnumerable<Message> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            Messages = messages.ToArray();
        }

        public Message[] Messages { get; }
    }
}