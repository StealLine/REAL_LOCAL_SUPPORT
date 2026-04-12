using Discord;
using Discord.WebSocket;
using Support_Bot.MODELS_FOR_DISCORD_BOT;

namespace Support_Bot.DISCORDBOT
{
    public static class ErrorMessageClass
    {
        public static async Task ErrorMessageLogToUser(string message, ulong channel_id, DiscordSocketClient socketClient)
        {

            var channel = await socketClient.GetChannelAsync(channel_id);

            if (channel is ITextChannel chan)
            {
                if (chan == null)
                {
                    return;
                }
                var embed = new EmbedBuilder().WithDescription($"Error happened in bot codebase: \n\n{message}\n\n Error Date UTC: {DateTime.UtcNow.ToShortDateString}").WithColor(Color.Red).Build();
                await chan.SendMessageAsync($"<@&{DISCORD_CRED.ROLE_FOR_TICKETPING}>",embed:embed);
            }
            else
            {
                return;
            }

        }
    }
}
