using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_METHODS.ContextAndHelpers;

namespace Support_Bot.DB_METHODS
{
    public class DB_CHECK_ACTIVITY: IcheckActivity
    {
        private readonly ILogger<DB_CHECK_ACTIVITY> _logger;
        private readonly ContextForDB _contextForDB;
        public DB_CHECK_ACTIVITY (ILogger<DB_CHECK_ACTIVITY> logger, ContextForDB contextForDB)
        {
            _logger = logger;
            _contextForDB = contextForDB;
        }

        public async Task<string> Check(string userID)
        {
            ulong total = 0;
            var retries = 6;
            var delayBase = 10000;

            for (int attempt = 1; attempt <= retries; attempt++)
            {

                try
                {
                    bool exists = await _contextForDB.TicketLogs
                        .AnyAsync(x => x.isactive && x.ticket_creator_id == userID);

                    return exists ? "true" : "false";

                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"DB connection attempt {attempt} failed in DB_CHECK_ACTIVITY: {ex.Message}");
                    if (attempt == retries)
                    {
                        _logger.LogCritical($"MAX RETRIES REACHED IN DB_CHECK_ACTIVITY");
                        return "ERROR IN DB_CHECK_ACTIVITY";
                    }
                    await Task.Delay(delayBase * (int)Math.Pow(2, attempt - 1));
                }
            }
            return "UNKNOW ERROR IN DB_CHECK_ACTIVITY";
        }
    }
}
