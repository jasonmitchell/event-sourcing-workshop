using Kanban.Framework;

namespace Kanban.Domain.Cards.Commands
{
    public class StartDevelopment : Command
    {
        public string Id { get; set; }
    }
}