using Discord.WebSocket;

namespace Support_Bot.SENDTXT
{
    public interface IdiscordSendTXT
    {
        public Task<bool> Send(ulong userID, string ticketID, DiscordSocketClient discordSocketClient);
    }
}
