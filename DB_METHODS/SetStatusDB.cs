using Microsoft.EntityFrameworkCore;
using Npgsql;
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_METHODS.ContextAndHelpers;
using Support_Bot.DB_METHODS.Entitys;
using System.Data.Common;
using System.Text;

namespace Support_Bot.DB_METHODS
{
    public class SetStatusDB: ISetStatus
    {
        private readonly ContextForDB contextForDB;
        private readonly ILogger<SetStatusDB> _logger;
        public SetStatusDB(ContextForDB contextForDB, ILogger<SetStatusDB> logger)
        {
            this.contextForDB = contextForDB;
            _logger = logger;
        }
        public async Task<bool> SetStat(string ticket_id)
        {

            var retries = 6;
            var delayBase = 10000;

            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {

                    await contextForDB.TicketLogs
                        .Where(t => t.ticket_id == ticket_id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(t => t.isactive, t => false)
                            .SetProperty(t => t.time_closed, t => DateTime.UtcNow)
                        );

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"DB attempt {attempt} failed in SetStatus: {ex.Message}");
                    if (attempt == retries)
                    {
                        _logger.LogCritical("MAX RETRIES REACHED in SetStatus");
                        return false;
                    }

                    await Task.Delay(delayBase * (int)Math.Pow(2, attempt - 1));
                }
            }

            return false;
        }
    }
}
