using System.Threading.Tasks;

namespace Kanban.Framework
{
    /// <summary>
    /// An interface for implementing command handlers
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<TCommand> where TCommand : Command
    {
        /// <summary>
        /// Implements logic for handling the provided command.
        /// </summary>
        /// <param name="command"></param>
        Task Handle(TCommand command);
    }
}