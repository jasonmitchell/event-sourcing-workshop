using System.Threading.Tasks;

namespace Kanban.Domain.Cards.Queries
{
    public interface ICardActivityRepository
    {
        Task AppendActivityToCard(string cardId, string activity);
        Task<string[]> GetActivityForCard(string cardId);
    }
}