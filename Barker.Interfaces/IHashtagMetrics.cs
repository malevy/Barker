using System.Threading.Tasks;
using Orleans;
using Barker.Interfaces;

namespace Barker.Interfaces
{
    public interface IHashtagMetrics : IGrainWithStringKey
    {
        Task IncrementOccurance();
        Task Reset();
        Task<OccuranceMetric> GetCurrentMetrics();

        Task<string> Hashtag();
    }
}