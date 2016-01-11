using System;
using System.Threading.Tasks;
using Orleans;

namespace Barker.Interfaces
{
    public interface IAccount : IGrainWithGuidKey, IGrainObserver
    {
        Task Publish(string text);
        Task<Guid> AccountID();
    }
}