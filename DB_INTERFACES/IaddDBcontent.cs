using Support_Bot.DB_METHODS.Entitys;
using Support_Bot.DISCORDBOT.DISCORDMODELS;

namespace Support_Bot.DB_INTERFACES
{
    public interface IaddDBcontent
    {
        public Task<bool> AddContent(Dictionary<ulong, TicketLogs> snap);
    }
}
