using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Orleans;

namespace Barker.Interfaces
{
    public interface IMessageHub : IGrainWithIntegerKey
    {
        Task Publish(Message message);
        Task RegisterAccount(Guid accountId, IAccount account);
        Task UnregisterAccount(Guid accountId);

    }
}