
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_METHODS.Entitys;
using Support_Bot.DISCORDBOT.DISCORD_MAIN;
using Support_Bot.DISCORDBOT.DISCORDMODELS;

namespace Support_Bot.DISCORDBOT
{

    public class BACKGROUND_ADD_MESSAGE_TO_DB : BackgroundService
    {
        public static bool isBusy = false;
        DateTime date = DateTime.UtcNow.AddSeconds(30);
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<BACKGROUND_ADD_MESSAGE_TO_DB> logger;
        public static readonly object tempMessagesLock = new();
        public BACKGROUND_ADD_MESSAGE_TO_DB(IServiceScopeFactory serviceScopeFactory, ILogger<BACKGROUND_ADD_MESSAGE_TO_DB> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow > date)
                    {
                        logger.LogInformation("Background service adding history to db started");
                        isBusy = true;
                        using (var scope = serviceScopeFactory.CreateScope())
                        {
                            var dbadd = scope.ServiceProvider.GetRequiredService<IaddDBcontent>();

                            var snapshot = new Dictionary<ulong, TicketLogs>();

                            if (DISCORD_MAIN_METHOD.temp_messages.Count > 0)
                            {

                                lock (tempMessagesLock)
                                {
                                    snapshot = DISCORD_MAIN_METHOD.temp_messages.ToDictionary(x => x.Key, x => x.Value);

                                    DISCORD_MAIN_METHOD.temp_messages.Clear();
                                }

                            }

                            foreach (var item in DISCORD_MAIN_METHOD.users)
                            {

                                if (!snapshot.TryGetValue(item.Key, out var userTemp))
                                {
                                    userTemp = new TicketLogs();
                                    snapshot[item.Key] = userTemp;
                                }

                                var src = item.Value;

                                if (!string.IsNullOrEmpty(src.edited_message))
                                {
                                    userTemp.ticket_edited_messages += src.edited_message;
                                    src.edited_message = null;
                                }

                                if (!string.IsNullOrEmpty(src.message_history))
                                {
                                    userTemp.ticket_content += src.message_history;
                                    src.message_history = null;
                                }

                                if (!string.IsNullOrEmpty(src.deleted_message))
                                {
                                    userTemp.ticket_deleted_messages += src.deleted_message;
                                    src.deleted_message = null;
                                }
                            }

                            foreach (var kvp in snapshot)
                            {
                                Console.WriteLine($"Key (ulong): {kvp.Key}");
                                var ticket = kvp.Value;

                                Console.WriteLine($"Id: {ticket.Id}");
                                Console.WriteLine($"Time Created: {ticket.time_created}");
                                Console.WriteLine($"Time Closed: {ticket.time_closed}");
                                Console.WriteLine($"Ticket Content: {ticket.ticket_content}");
                                Console.WriteLine($"Ticket Deleted Messages: {ticket.ticket_deleted_messages}");
                                Console.WriteLine($"Ticket Edited Messages: {ticket.ticket_edited_messages}");
                                Console.WriteLine($"Ticket Creator Id: {ticket.ticket_creator_id}");
                                Console.WriteLine($"Ticket Type: {ticket.ticket_type}");
                                Console.WriteLine($"Ticket Id: {ticket.ticket_id}");
                                Console.WriteLine($"Is Active: {ticket.isactive}");
                                Console.WriteLine("------------------------------");
                            }

                            var res = await dbadd.AddContent(snapshot);
                            logger.LogInformation("Backround finished successfully");

                        }

                        date = DateTime.UtcNow.AddSeconds(30);

                    }
                    if (isBusy)
                        isBusy = false;
                    await Task.Delay(7000);
                }catch (Exception ex)
                {
                    logger.LogCritical("unexpected error happened in background service");
                    Console.WriteLine(ex);
                }
            }

        }
    }

}
