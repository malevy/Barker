using System.Threading.Tasks;
using Barker.Interfaces;
using Orleans;

namespace Barker.Grains
{
    public class HasttagMetricsState : GrainState
    {
        public int Count { get; set; }
    }

    public class HashtagMetrics : Grain<HasttagMetricsState>, IHashtagMetrics
    {
        public async Task IncrementOccurance()
        {
            this.State.Count++;
            await this.WriteStateAsync();
        }

        public async Task Reset()
        {
            this.State.Count = 0;
            await this.WriteStateAsync();
        }

        public Task<OccuranceMetric> GetCurrentMetrics() => Task.FromResult(new OccuranceMetric(this.GetPrimaryKeyString(), this.State.Count));

        public Task<string> Hashtag() => Task.FromResult(this.GetPrimaryKeyString());

        public override Task OnActivateAsync()
        {
            var collectionProxy = GrainFactory.GetGrain<ITrendingHashtags>(0);
            collectionProxy.Register(this);
            return base.OnActivateAsync();
        }

    }
}