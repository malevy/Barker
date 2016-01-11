using System.Threading.Tasks;
using Orleans;

namespace Barker.Interfaces
{
    public interface IHashtagPublisher : IGrainWithStringKey
    {
        Task<string> ProcessMessage(string message);
    }
}