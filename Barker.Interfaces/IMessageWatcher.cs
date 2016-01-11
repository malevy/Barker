using System.Threading.Tasks;
using Orleans;

namespace Barker.Interfaces
{
    public interface IMessageWatcher : IGrainObserver
    {
        void Receive(Message[] messages);
    }
}