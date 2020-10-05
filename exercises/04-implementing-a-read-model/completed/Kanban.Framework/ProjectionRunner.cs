using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.Logging;

namespace Kanban.Framework
{
    /// <summary>
    /// Subscribes to EventStoreDB and executes projections
    /// </summary>
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
        
        /// <summary>
        /// Start the subscription to EventStoreDB 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task Start(CancellationToken cancellationToken)
        {
            var filter = new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents());

            _subscription = await _client.SubscribeToAllAsync(Position.Start,
                                                              EventAppeared,
                                                              subscriptionDropped: SubscriptionDropped,
                                                              filterOptions: filter,
                                                              resolveLinkTos: true,
                                                              cancellationToken: cancellationToken);
        }

        private async Task EventAppeared(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{eventType}@{preparePosition}", resolvedEvent.Event.EventType, resolvedEvent.Event.Position.PreparePosition);

            var e = EventSerializer.Deserialize(resolvedEvent);
            await ExecuteProjectionHandlers(e);
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
        
        private void SubscriptionDropped(StreamSubscription subscription, SubscriptionDroppedReason subscriptionDroppedReason, Exception ex)
        {
            _logger.LogError(ex, "Subscription Dropped ({subscriptionDroppedReason})", subscriptionDroppedReason);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}