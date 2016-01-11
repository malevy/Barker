using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Barker.Interfaces;
using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;

namespace Barker.Grains
{
    [PreferLocalPlacement, Reentrant]
    public class HashtagPublisher : Grain, IHashtagPublisher
    {
        public async Task<string> ProcessMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return message;

            var hashTagMatches = Regex.Matches(message, @"(?:(?<=\s)|^)#(\w*[A-Za-z_]+\w*)");

            var requests = hashTagMatches
                .OfType<Match>()
                .Select(m => GrainFactory.GetGrain<IHashtagMetrics>(m.Value))
                .Select(a => a.IncrementOccurance())
                .ToList();

            await Task.WhenAll(requests);
            return message;
        }
    }
}