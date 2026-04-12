using Npgsql;
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_METHODS.ContextAndHelpers;
using Support_Bot.DB_METHODS.Entitys;

namespace Support_Bot.DB_METHODS
{
    public class DB_ADD_TICKET_INFO : IDB_TICKER_CREATED
    {
        private readonly ILogger<DB_ADD_TICKET_INFO> _logger;
        private readonly ContextForDB contextForDB;
        public DB_ADD_TICKET_INFO(ContextForDB contextForDB, ILogger<DB_ADD_TICKET_INFO> logger)
        {
            this.contextForDB = contextForDB;
            _logger = logger;
        }
        public async Task<string> DB_ADD(string creatorID, string ticketID, string ticket_type)
        {
            var retries = 6;
            var delayBase = 10000;
            for (int attempt = 1; attempt <= retries; attempt++)
            {

                try
                {

                    TicketLogs logs = new TicketLogs
                    {
                        time_created = DateTime.UtcNow,
                        ticket_creator_id = creatorID,
                        ticket_type = ticket_type,
                        ticket_id = ticketID,
                        isactive = true,
                    };
                    await contextForDB.TicketLogs.AddAsync(logs);

                    await contextForDB.SaveChangesAsync();

                    return "true";

                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"DB connection attempt {attempt} failed in DB_ADD_TICKET_INFO: {ex.Message}");
                    if (attempt == retries)
                    {
                        _logger.LogCritical($"MAX RETRIES REACHED IN DB_ADD_TICKET_INFO");
                        return "Error in DB_ADD_TICKET_INFO";
                    }
                    await Task.Delay(delayBase * (int)Math.Pow(2, attempt - 1));
                }
            }
            return "Unknown error in DB_ADD_TICKET_INFO";
        }
    }
}
