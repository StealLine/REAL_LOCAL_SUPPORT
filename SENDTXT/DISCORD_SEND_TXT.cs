using Discord;
using Discord.WebSocket;
using Support_Bot.MODELS_FOR_DISCORD_BOT;

namespace Support_Bot.SENDTXT
{
    public class DISCORD_SEND_TXT : IdiscordSendTXT
    {
        private readonly IdiscordGetTXT idiscordGetTXT;
        private readonly ILogger<DISCORD_SEND_TXT> _logger;
        public DISCORD_SEND_TXT(IdiscordGetTXT idiscordGetTXT, ILogger<DISCORD_SEND_TXT> logger)
        {
            this.idiscordGetTXT = idiscordGetTXT;
            this._logger = logger;
        }
        public async Task<bool> Send(ulong userID, string ticketID, DiscordSocketClient discordSocketClient)
        {
            string TicketText = await idiscordGetTXT.GetTXT(ticketID);
            if(TicketText == string.Empty)
            {
                _logger.LogCritical("Ticket content is null, or an error happened");
                return false;
            }
            var user = await discordSocketClient.Rest.GetGuildUserAsync(DISCORD_CRED.GuildID,userID);
            if (user == null)
            {
                _logger.LogCritical("User is not on a server ");
                return false;
            }

            try
            {
                string path =  $"ticket-transcript-{ticketID}.txt";
                await File.WriteAllTextAsync(path, TicketText);

                var dm = await user.CreateDMChannelAsync();

                var emb = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithAuthor("Guard Auto MM", "https://i.ibb.co/jvZF7JWp/Untitled1620-20250913185940.png",
                        "https://discord.com/invite/snvPNYGTxU")
                    .WithTitle("Ticket Transcript")
                    .WithDescription($"<@{userID}> thank you for contacting our support team! We hope we were able to assist you with your questions.\n" +
                                     $"Here’s a copy of your ticket transcript for your reference.")
                    .Build();

                await dm.SendMessageAsync(embed: emb);

                await dm.SendFileAsync(path);

                if (File.Exists(path)) File.Delete(path);

            }
            catch (Discord.Net.HttpException ex) when (ex.DiscordCode == Discord.DiscordErrorCode.CannotSendMessageToUser)
            {
                _logger.LogCritical("Private messages blocked");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return false;
            }
            return true;

        }
    }
}
