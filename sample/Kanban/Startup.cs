using System;
using System.Net.Http;
using EventStore.Client;
using Kanban.Domain.Cards;
using Kanban.Domain.Cards.Commands;
using Kanban.Domain.Cards.Queries;
using Kanban.Domain.Cards.Queries.Projections;
using Kanban.Framework;
using Kanban.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kanban
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddSingleton(_ => CreateEventStoreClient())
                    .AddTransient<IEventStore, EsdbEventStore>();

            services.AddTransient<ICommandHandler<OpenTask>, Handlers>()
                    .AddTransient<ICommandHandler<AssignCard>, Handlers>()
                    .AddTransient<ICommandHandler<StartDevelopment>, Handlers>()
                    .AddTransient<CardAggregateStore>()
                    .AddSingleton<ICardActivityRepository, InMemoryCardActivityRepository>();

            services.AddHostedService<ProjectionService>()
                    .AddTransient<CardActivityProjection>()
                    .AddSingleton(sp => new Projection[]
                    {
                        sp.GetRequiredService<CardActivityProjection>()
                    })
                    .AddSingleton<ProjectionRunner>();
        }
        
        private static EventStoreClient CreateEventStoreClient()
        {
            return new EventStoreClient(new EventStoreClientSettings
            {
                ConnectivitySettings =
                {
                    Address = new Uri("http://localhost:2113"),
                },
                DefaultCredentials = new UserCredentials("admin", "changeit")
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting()
               .UseAuthorization()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });
        }
    }
}