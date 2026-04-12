using Support_Bot.MODELS_FOR_DISCORD_BOT;
using Discord.WebSocket;
using Discord;
using Support_Bot.DISCORDBOT.DISCORD_MAIN;
using Support_Bot.DISCORDBOT.INTERFACES_DISCORD;

namespace Support_Bot.DISCORDBOT
{
    public class DISCORD_START : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient discordSocketClient;
        private DISCORD_MAIN_METHOD dISCORD_MAIN_METHOD;
        private readonly ILogger<DISCORD_MAIN_METHOD> logger;
        private readonly HttpClient httpClient;
        public DISCORD_START(DiscordSocketClient discordSocketClient, HttpClient httpClient,
            IServiceProvider serviceProvider, ILogger<DISCORD_MAIN_METHOD> logger)
        {
            this.discordSocketClient = discordSocketClient;
            this.httpClient = httpClient;
            this._serviceProvider = serviceProvider;
            this.logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Попав сюда");
            try
            {
                Console.WriteLine(DISCORD_CRED.DISCORDTOKEN);
                await discordSocketClient.LoginAsync(TokenType.Bot, DISCORD_CRED.DISCORDTOKEN);
                await discordSocketClient.StartAsync();

                using(var client = _serviceProvider.CreateScope())
                {
                    var ticket_creation = client.ServiceProvider.GetRequiredService<IhandleTicketCreation>();
                    dISCORD_MAIN_METHOD = new DISCORD_MAIN_METHOD(httpClient, discordSocketClient,ticket_creation,logger);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return discordSocketClient.StopAsync();
        }
    }
}
