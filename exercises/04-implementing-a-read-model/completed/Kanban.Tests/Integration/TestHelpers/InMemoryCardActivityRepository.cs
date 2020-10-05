using System.Collections.Generic;
using System.Threading.Tasks;
using Kanban.Domain.Cards.Queries;

namespace Kanban.Tests.Integration.TestHelpers
{
    public class InMemoryCardActivityRepository : ICardActivityRepository
    {
        private readonly Dictionary<string, List<string>> _activityStreams = new Dictionary<string, List<string>>();
        
        public Task AppendActivityToCard(string cardId, string activity)
        {
            if (!_activityStreams.TryGetValue(cardId, out var activityStream))
            {
                activityStream = new List<string>();
                _activityStreams[cardId] = activityStream;
            }
            
            activityStream.Add(activity);
            return Task.CompletedTask;
        }

        public Task<string[]> GetActivityForCard(string cardId)
        {
            if (_activityStreams.ContainsKey(cardId))
            {
                var activityStream = _activityStreams[cardId].ToArray();
                return Task.FromResult(activityStream);
            }

            return Task.FromResult<string[]>(null);
        }
    }
}