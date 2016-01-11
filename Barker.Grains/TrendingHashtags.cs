using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Barker.Interfaces;
using Orleans;

namespace Barker.Grains
{
    public class TrendingHashtagsState : GrainState
    {
        public List<IHashtagMetrics> KnownHashTags;
    }

    public class TrendingHashtags : Grain<TrendingHashtagsState>, ITrendingHashtags
    {
        private Dictionary<string, long> _trending;

        public override Task OnActivateAsync()
        {
            if (null == State.KnownHashTags) State.KnownHashTags = new List<IHashtagMetrics>();

            this.RegisterTimer(CacheTrending, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
            return WriteStateAsync();
        }

        public Task Register(IHashtagMetrics hashtagMetrics)
        {
            if (!this.State.KnownHashTags.Contains(hashtagMetrics))
            {
                this.State.KnownHashTags.Add(hashtagMetrics);
            }

            return WriteStateAsync();
        }

        public Task<Dictionary<string, long>> GetTrending() => Task.FromResult(_trending ?? new Dictionary<string, long>(0));

        public async Task Reset()
        {
            var requests = State.KnownHashTags.Select(ht => ht.Reset()).ToList();
            await Task.WhenAll(requests);
            this._trending = null;
        }

        private async Task CacheTrending(object state)
        {
            var requests = State.KnownHashTags.Select(ht => ht.GetCurrentMetrics()).ToList();
            var results = await Task.WhenAll(requests);
            this._trending = results
                .Where(m => m.Count > 0)
                .ToDictionary(m => m.Hashtag, m => m.Count);
        }
    }
}