using Kanban.Framework;

namespace Kanban.Domain.Cards.Commands
{
    public class AssignCard : Command
    {
        public string Id { get; set; }
        public string Assignee { get; set; }
    }
}