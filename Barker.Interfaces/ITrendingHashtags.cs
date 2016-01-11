using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Barker.Interfaces
{
    public interface ITrendingHashtags : IGrainWithIntegerKey
    {
        Task Register(IHashtagMetrics hashtagMetrics);
        Task<Dictionary<string, long>> GetTrending();
        Task Reset();
    }
}