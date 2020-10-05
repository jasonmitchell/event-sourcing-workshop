using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.Logging;

namespace Kanban.Framework
{
    public class ProjectionRunner : IDisposable
    {
        private readonly EventStoreClient _client;
        private readonly ILogger<ProjectionRunner> _logger;
        private readonly Projection[] _projections;
        
        private StreamSubscription _subscription;

        public ProjectionRunner(EventStoreClient client, ILogger<ProjectionRunner> logger,
                                 Projection[] projections)
        {
            _client = client;
            _logger = logger;
            _projections = projections;
        }
        
        public async Task Start(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        private async Task ExecuteProjectionHandlers(Event e)
        {
            foreach (var projection in _projections)
            {
                if (projection.CanHandle(e.GetType()))
                {
                    await projection.Handle(e);
                }
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}