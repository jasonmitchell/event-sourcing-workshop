using System.Threading;
using System.Threading.Tasks;
using Kanban.Framework;
using Microsoft.Extensions.Hosting;

namespace Kanban.Infrastructure
{
    public class ProjectionService : IHostedService
    {
        private readonly ProjectionRunner _runner;

        public ProjectionService(ProjectionRunner runner)
        {
            _runner = runner;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _runner.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}