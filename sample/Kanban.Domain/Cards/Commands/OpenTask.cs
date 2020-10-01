using Kanban.Framework;

namespace Kanban.Domain.Cards.Commands
{
    public class OpenTask : Command
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}