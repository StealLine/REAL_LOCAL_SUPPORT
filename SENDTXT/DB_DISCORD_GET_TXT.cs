using Microsoft.EntityFrameworkCore;
using Npgsql;
using Support_Bot.DB_METHODS.ContextAndHelpers;
using System.Data.Common;

namespace Support_Bot.SENDTXT
{
    public class DB_DISCORD_GET_TXT: IdiscordGetTXT
    {
        private readonly ContextForDB contextForDB;
        public DB_DISCORD_GET_TXT(ContextForDB contextForDB)
        {
            this.contextForDB = contextForDB;
        }
        public async Task<string> GetTXT(string ticketID)
        {
            var retries = 6;
            var delayBase = 10000;
            for (int attempt = 1; attempt <= retries; attempt++)
            {

                try
                {
                    string content = await contextForDB.TicketLogs.Where(x=>x.ticket_id == ticketID).Select(x=>x.ticket_content).FirstOrDefaultAsync()
                        ?? string.Empty;

                    return content;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB connection attempt {attempt} failed in DB_DISCORD_GET_TXT: {ex.Message}");
                    if (attempt == retries)
                    {
                        Console.WriteLine($"MAX RETRIES REACHED IN DB_DISCORD_GET_TXT");
                        return null;
                    }
                    await Task.Delay(delayBase * (int)Math.Pow(2, attempt - 1));
                }
            }
            Console.WriteLine($"Unknown error in DB_DISCORD_GET_TXT");
            return null;
        }
    }
}
