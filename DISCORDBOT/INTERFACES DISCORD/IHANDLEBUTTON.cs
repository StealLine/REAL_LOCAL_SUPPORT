using Discord.WebSocket;

namespace Support_Bot.DISCORDBOT.INTERFACES_DISCORD
{
    public interface IhandleTicketCreation
    {
        public Task CreateTicket(SocketMessageComponent socket, HttpClient httpClient, DiscordSocketClient discordSocket);
    }
}
