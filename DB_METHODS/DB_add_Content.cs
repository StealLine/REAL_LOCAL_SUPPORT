using Npgsql;
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_METHODS.ContextAndHelpers;
using Support_Bot.DB_METHODS.Entitys;
using Support_Bot.DISCORDBOT.DISCORD_MAIN;
using Support_Bot.DISCORDBOT.DISCORDMODELS;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Routing;
namespace Support_Bot.DB_METHODS
{
    public class DB_add_Content: IaddDBcontent
    {
        private readonly ContextForDB contextForDB;
        public DB_add_Content(ContextForDB contextForDB)
        {
            this.contextForDB = contextForDB;
        }

        public async Task<bool> AddContent(Dictionary<ulong, TicketLogs> snap)
        {
            if (snap == null || snap.Count == 0)
                return true;

            var retries = 6;
            var delayBase = 10000;

            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    var sql = new StringBuilder();
                    var parameters = new List<NpgsqlParameter>();

                    sql.Append("UPDATE \"TicketLogs\" SET ");

                    sql.Append("ticket_content = CASE ticket_id ");
                    int i = 0;
                    foreach (var kvp in snap)
                    {
                        sql.Append($"WHEN @id{i} THEN ticket_content || @msg{i} ");
                        parameters.Add(new NpgsqlParameter($"id{i}", kvp.Key.ToString()));
                        parameters.Add(new NpgsqlParameter($"msg{i}", kvp.Value.ticket_content ?? ""));
                        i++;
                    }
                    sql.Append("ELSE ticket_content END, ");

                    sql.Append("ticket_edited_messages = CASE ticket_id ");
                    i = 0;
                    foreach (var kvp in snap)
                    {
                        sql.Append($"WHEN @id{i} THEN ticket_edited_messages || @edit{i} ");
                        parameters.Add(new NpgsqlParameter($"edit{i}", kvp.Value.ticket_edited_messages ?? ""));
                        parameters.Add(new NpgsqlParameter($"id{i}", kvp.Key.ToString()));
                        i++;
                    }
                    sql.Append("ELSE ticket_edited_messages END, ");

                    sql.Append("ticket_deleted_messages = CASE ticket_id ");
                    i = 0;
                    foreach (var kvp in snap)
                    {
                        sql.Append($"WHEN @id{i} THEN ticket_deleted_messages || @del{i} ");
                        parameters.Add(new NpgsqlParameter($"del{i}", kvp.Value.ticket_deleted_messages ?? ""));
                        parameters.Add(new NpgsqlParameter($"id{i}", kvp.Key.ToString()));
                        i++;
                    }
                    sql.Append("ELSE ticket_deleted_messages END ");

                    sql.Append("WHERE ticket_id IN (");
                    sql.Append(string.Join(",", snap.Keys.Select((k, i) => $"@id{i}")));
                    sql.Append(");");

                    i = 0;
                    foreach (var kvp in snap)
                    {
                        parameters.Add(new NpgsqlParameter($"id{i}", kvp.Key.ToString()));
                        i++;
                    }

                    await contextForDB.Database.ExecuteSqlRawAsync(sql.ToString(), parameters);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB attempt {attempt} failed in AddContent: {ex.Message}");
                    if (attempt == retries)
                    {
                        Console.WriteLine("MAX RETRIES REACHED in AddContent");
                        return false;
                    }

                    await Task.Delay(delayBase * (int)Math.Pow(2, attempt - 1));
                }
            }

            return false;
        }
    }

}
