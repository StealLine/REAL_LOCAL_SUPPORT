using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Support_Bot.CREDENTIALS_CLASSES;
using Support_Bot.DB_MODELS;
using Support_Bot.DISCORDBOT.DISCORDMODELS;
using Support_Bot.DISCORDBOT.INTERFACES_DISCORD;
using Support_Bot.MANAGEMENT_CLASSES;
using Support_Bot.MODELS_FOR_DISCORD_BOT;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;

namespace Support_Bot.DISCORDBOT.DISCORD_MAIN
{
    public class DISCORD_HANDLE_TICKET_CREATION: IhandleTicketCreation
    {
        private readonly ILogger<DISCORD_HANDLE_TICKET_CREATION> logger;
        public DISCORD_HANDLE_TICKET_CREATION(ILogger<DISCORD_HANDLE_TICKET_CREATION> logger)
        {
            this.logger = logger;
        }
        public async Task CreateTicket(SocketMessageComponent socket, HttpClient httpClient, DiscordSocketClient discordSocket)
        {
            try
            {
                string ticket_type = "UNKNOWN ERROR HAPPENED, PING ADMIN";
                var user = socket.User.Id;

                var req2 = await httpClient.GetAsync($"{REQ_URL.URL}/api/Home/CHECKACTIVE?secret={Secret.Secret_Key}&userID={user.ToString()}");
                var response2 = await req2.Content.ReadAsStringAsync();
                if (!req2.IsSuccessStatusCode)
                {
                    logger.LogCritical("Cant check activity of a user in DISCORD_HANDLE_TICKET_CREATION");
                    var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**An error occurred while checking active tickets.**\n" +
                         $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> for further assistance or try again later").Build();
                    await socket.FollowupAsync(embed: emb,ephemeral:true);
                    return;
                }

                bool success = bool.TryParse(response2, out bool isbusy);
                if (!success)
                {
                    logger.LogCritical("Cant check activity of a user in DISCORD_HANDLE_TICKET_CREATION PARSER");
                    var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**An error occurred while checking active tickets.**\n" +
                         $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> for further assistance").Build();
                    await socket.FollowupAsync(embed: emb, ephemeral: true);
                    return;
                }
                logger.LogInformation($"isbusy = {isbusy} in DISCORD_HANDLE_TICKET_CREATION");
                if (isbusy)
                {
                    var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription("**You already have an active support ticket. " +
                        "Please close it before creating a new one**").Build();
                    await socket.FollowupAsync(embed: emb, ephemeral: true);
                    return;
                }

                var selected_opt = socket.Data.Values.First();
                if (selected_opt == "GENERAL")
                {
                    ticket_type = "General ticket";
                }
                else if (selected_opt == "ISSUES")
                {

                    ticket_type = "Bot Issues ticket";
                }
                else if (selected_opt == "ADMIN")
                {

                    ticket_type = "Admin ticket";
                }
                else if (selected_opt == "OTHER")
                {

                    ticket_type = "Other ticket";
                }

                var guild = discordSocket.GetGuild(DISCORD_CRED.GuildID);
                if (guild == null)
                {
                    logger.LogCritical("No such guild in DISCORD_HANDLE_TICKET_CREATION");
                    return;

                }
                var category = guild.CategoryChannels.FirstOrDefault(c => c.Id == DISCORD_CRED.SUPPORT_CATEGORY);
                if (category == null)
                {
                    logger.LogCritical("No such category in the server DISCORD_HANDLE_TICKET_CREATION");
                    return;
                }

                string ticket_name = $"support-ticket-{ticket_type}-{user}";
                RestTextChannel? channel = null;

                channel = await guild.CreateTextChannelAsync(ticket_name, props =>
                {
                    props.CategoryId = category.Id;
                    props.SlowModeInterval = 5;
                    var overwrites = new List<Overwrite>
                    {

                        new Overwrite(
                            guild.EveryoneRole.Id,
                            PermissionTarget.Role,
                            new OverwritePermissions(
                                viewChannel: PermValue.Deny,
                                sendMessages: PermValue.Deny
                            )
                        ),

                    };
                    foreach (var roleId in DISCORD_CRED.DISCORD_SUPPORT_ROLES)
                    {
                        overwrites.Add(new Overwrite(
                            roleId,
                            PermissionTarget.Role,
                            new OverwritePermissions(
                                viewChannel: PermValue.Allow,
                                sendMessages: PermValue.Allow
                            )
                        ));
                    }

                    props.PermissionOverwrites = overwrites;
                });

                await channel.AddPermissionOverwriteAsync(socket.User, DISCORD_MAIN_METHOD.perms);
                logger.LogInformation("Channel created, permissions overwrited");

                var API_MODEL = new DB_Ticket_Created_Model_Body
                {
                    creator_id = user.ToString(),
                    ticket_id = channel.Id.ToString(),
                    ticket_type = ticket_type,
                };

                var json = System.Text.Json.JsonSerializer.Serialize(API_MODEL);

                var cont = new StringContent(json, encoding: Encoding.UTF8, "application/json");

                var req = await httpClient.PostAsync($"{REQ_URL.URL}/api/Home/DB_Ticket_Created?secret={Secret.Secret_Key}", cont);

                var response = await req.Content.ReadAsStringAsync();
                if (!req.IsSuccessStatusCode)
                {
                    logger.LogCritical("Error with DB_Ticket_Created method API ");
                    var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**An error occurred while collecting information about you.**\n" +
                    $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> for further assistance. \n\n Error message: {response}").Build();
                    await socket.FollowupAsync(embed: emb, ephemeral: true);
                    return;
                }

                USER_MODEL model = new USER_MODEL
                {
                    ticket_id = channel.Id,
                    ticket_type = ticket_type,
                    user_id = user,
                };

                DISCORD_MAIN_METHOD.users.TryAdd(channel.Id, model);

                var creationTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'");
                var embed = new EmbedBuilder()
                    .WithColor(Color.DarkGrey)
                    .WithTitle("🎫 Support Ticket Created")
                    .WithDescription(
                        $"Hello <@{socket.User.Id}>!\n\n" +
                        $"**Ticket Type:** ``{ticket_type}``\n" +
                        $"**Created by:** ``{socket.User.Username} ({socket.User.Id})``\n" +
                        $"**Created at:** ``{creationTime}``\n\n" +
                        "Please describe your question or issue in as much detail as possible so our support team can assist you efficiently."
                    )
                    .WithThumbnailUrl(socket.User.GetAvatarUrl() ?? socket.User.GetDefaultAvatarUrl())
                    .WithFooter("Our support team will respond as soon as possible.")
                    .Build();

                var comp = new ComponentBuilder().WithButton(label: "🔒 Close ticket", customId: "CL", ButtonStyle.Danger).Build();

                await channel.SendMessageAsync(
                             ticket_type == "Admin ticket"
                            ? $"<@&{DISCORD_CRED.ADMINROLE}>"
                            : $"<@&{DISCORD_CRED.ROLE_FOR_TICKETPING}>",
                        embed: embed,
                        components: comp
                    );
                var embedTicketCreatEND = new EmbedBuilder()
                        .WithColor(Color.Green)
                        .WithDescription($"✅ Success! Ticket was created: <#{channel.Id}>")
                        .Build();
                await socket.FollowupAsync(embed: embedTicketCreatEND, ephemeral: true);
                logger.LogInformation("Message sent, method successfully finished their part.");

            }
            catch (Discord.Net.HttpException httpEx)
            {

                if ((int)httpEx.HttpCode == 429)
                {
                    logger.LogWarning("Someone is spamming tickets");
                    var emb = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithDescription("**You are creating tickets too frequently.**\n" +
                                         "Please wait a few seconds before trying again")
                        .Build();
                    await socket.FollowupAsync(embed: emb, ephemeral: true);
                    return;
                }

                if ((int)httpEx.HttpCode == 400 || (int)httpEx.HttpCode == 500)
                {
                    if (httpEx.Message.Contains("maximum number of channels in category", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogWarning("Maximum tickets reached in category");
                        var emb = new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithDescription("**Cannot create ticket.**\n" +
                                             "The support category has reached the maximum number of channels.\n" +
                                             $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> to resolve this.")
                            .Build();
                        await socket.FollowupAsync(embed: emb, ephemeral: true);
                        return;
                    }
                }
                logger.LogWarning("UNEXPECTED ERROR");
                Console.WriteLine(httpEx.Message);
                var embUnknown = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"**An unexpected Discord API error occurred.**\n" +
                                     $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}>\n\n")
                    .Build();

                await socket.FollowupAsync(embed: embUnknown, ephemeral: true);
                return;
            }
            catch (Exception ex)
            {
                logger.LogCritical("Unknown error happened, try catch global");
                Console.WriteLine(ex);

                var emb = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"**Some unknown error happened.**\n" +
                                     $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> for further assistance")
                    .Build();

                await socket.FollowupAsync(embed: emb, ephemeral: true);
                return;
            }

        }
    }
}
