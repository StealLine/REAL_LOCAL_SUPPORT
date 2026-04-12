using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Components.Forms;
using Support_Bot.CREDENTIALS_CLASSES;
using Support_Bot.DB_METHODS;
using Support_Bot.DB_METHODS.Entitys;
using Support_Bot.DB_MODELS;
using Support_Bot.DISCORDBOT.DISCORDMODELS;
using Support_Bot.DISCORDBOT.INTERFACES_DISCORD;
using Support_Bot.MANAGEMENT_CLASSES;
using Support_Bot.MODELS_FOR_DISCORD_BOT;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace Support_Bot.DISCORDBOT.DISCORD_MAIN
{

    public class DISCORD_MAIN_METHOD
    {
        private readonly IhandleTicketCreation ticketCreation;
        private readonly HttpClient _httpClient;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<DISCORD_MAIN_METHOD> logger;
        private ulong GlobalID = 0;
        public static bool isDELdetectionOK = true;
        public static bool isUPDdetectionOK = true;

        public static OverwritePermissions perms = new OverwritePermissions(
                  viewChannel: PermValue.Allow,
                  sendMessages: PermValue.Allow,
                  readMessageHistory: PermValue.Allow,
                  addReactions: PermValue.Deny,
                  useApplicationCommands: PermValue.Deny,
                  useExternalEmojis: PermValue.Deny,
                  useExternalStickers: PermValue.Deny,
                  embedLinks: PermValue.Deny,
                  createInstantInvite: PermValue.Deny,
                  startEmbeddedActivities: PermValue.Deny,
                  mentionEveryone: PermValue.Deny,
                 manageMessages: PermValue.Deny,
                 createPublicThreads: PermValue.Deny,
                 createPrivateThreads: PermValue.Deny,
                 sendMessagesInThreads: PermValue.Deny,
                 sendPolls: PermValue.Deny,
                 useExternalSounds: PermValue.Deny,
                 useSoundboard: PermValue.Deny

              );

        public static ConcurrentDictionary<ulong, USER_MODEL> users = new ConcurrentDictionary<ulong, USER_MODEL>();
        public static ConcurrentDictionary<ulong, TicketLogs> temp_messages = new ConcurrentDictionary<ulong, TicketLogs>();
        public DISCORD_MAIN_METHOD(HttpClient httpClient, DiscordSocketClient discordSocketClient, IhandleTicketCreation ticketCreation, ILogger<DISCORD_MAIN_METHOD> logger)
        {
            this.logger = logger;
            _httpClient = httpClient;
            this._client = discordSocketClient;
            this.ticketCreation = ticketCreation;
            _client.Ready += ReadyAsync;
            _client.ButtonExecuted += HandleButton;
            _client.SelectMenuExecuted += HandleMenu;
            _client.MessageDeleted += HandleDelete;
            _client.MessageUpdated += HandleUpdate;
            _client.MessageReceived += HandleMessage;

        }

        private async Task HandleMessage(SocketMessage message)
        {
            logger.LogInformation("Message received");
            if (message.Channel is ITextChannel chan)
            {
                if (chan.CategoryId != DISCORD_CRED.SUPPORT_CATEGORY)
                {
                    logger.LogInformation("Message not in a caregory");
                    return;

                }

                string title = "";
                string desc = "";
                if (message.Embeds.Count > 0)
                {
                    var em = message.Embeds.FirstOrDefault();
                    title = em.Title;
                    desc = em.Description;
                }
                string author = message.Author.IsBot ? "BOT" : $"{message.Author.Username} ({message.Author.Id})";

                string content = message.Embeds.Count == 0 ? message.Content.Replace("\n", " ") : $"[EMBED TITLE] {title} [EMBED DESCRIPTION] {desc}";

                string final = $"TIME({DateTime.UtcNow}) {author}: {content}";

                if (BACKGROUND_ADD_MESSAGE_TO_DB.isBusy == false)
                {
                    logger.LogInformation("Back is not busy. Default method working");
                    if (temp_messages.Any(x => x.Value.ticket_content != string.Empty) == true)
                    {
                        logger.LogInformation("Tempmessages is not empty");
                        try
                        {
                            foreach (var item in temp_messages)
                            {
                                if (item.Value.ticket_content == string.Empty)
                                    continue;

                                users.TryGetValue(item.Key, out var user);
                                if (user != null)
                                {
                                    user.message_history += item.Value.ticket_content + "\n";
                                    item.Value.ticket_content = string.Empty;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning("Error happened in temp messages in main discord");
                            logger.LogCritical(ex.Message);
                        }

                    }
                    users.TryGetValue(message.Channel.Id, out var val);
                    if (val == null)
                    {
                        logger.LogCritical($"{DateTime.UtcNow} No value for  channel in dict {message.Channel.Id}");
                        await ErrorMessageClass.ErrorMessageLogToUser($"No value for  channel in dict {message.Channel.Id}", message.Channel.Id, _client);
                        return;
                    }

                    logger.LogInformation($"{DateTime.UtcNow} Adding message: {final}");
                    val.message_history += final + "\n";
                }
                else
                {
                    logger.LogInformation("Background is busy temp messages working in HandleMessage");
                    logger.LogInformation($"{DateTime.UtcNow} Adding message to temp message: {final}");

                    lock (BACKGROUND_ADD_MESSAGE_TO_DB.tempMessagesLock)
                    {
                        if (!temp_messages.TryGetValue(message.Channel.Id, out var val) || val == null)
                        {
                            val = new TicketLogs();
                            temp_messages[message.Channel.Id] = val;
                        }

                        val.ticket_content += final + "\n";
                    }
                }
            }
        }

        private async Task HandleUpdate(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
        {
            if (isUPDdetectionOK)
            {
                try
                {
                    if (!cacheable.HasValue)
                    {
                        logger.LogWarning($"No value in cachable for {message.Content} in HandleUpdate");
                        return;
                    }

                    var oldmessage = cacheable.Value;
                    if (oldmessage.Author.IsBot)
                    {
                        logger.LogInformation($"Author is bot in HandleUpdate");
                        return;
                    }

                    if (channel is ITextChannel chan)
                    {
                        if (!chan.CategoryId.HasValue)
                        {
                            logger.LogWarning($"Channel with no category in HandleUpdate");
                            return;
                        }

                        if (chan.CategoryId == DISCORD_CRED.SUPPORT_CATEGORY)
                        {
                            var key = DISCORD_MAIN_METHOD.users.FirstOrDefault(x => x.Value.ticket_id == channel.Id).Key;
                            if (key == default)
                            {
                                logger.LogCritical($"{DateTime.UtcNow} No ticket with id {channel.Id} in dictionary users");
                                await ErrorMessageClass.ErrorMessageLogToUser($"No ticket with id {channel.Id} in dictionary users", channel.Id, _client);
                                return;
                            }

                            try
                            {

                                string final_cont_prev = SafeString.Safe(oldmessage.Content);
                                string final_cont_curr = SafeString.Safe(message.Content);
                                string final = string.Empty;
                                final = $"AUTHOR({oldmessage.Author}). TIME({DateTime.UtcNow}) orig: {final_cont_prev} || edited: {final_cont_curr}";

                                if (BACKGROUND_ADD_MESSAGE_TO_DB.isBusy == false)
                                {
                                    users.TryGetValue(message.Channel.Id, out var val);
                                    if (val == null)
                                    {
                                        logger.LogCritical($"{DateTime.UtcNow} No value for  channel in dict {message.Channel.Id}");
                                        await ErrorMessageClass.ErrorMessageLogToUser($"No value for  channel in dict {message.Channel.Id}", message.Channel.Id, _client);
                                        return;
                                    }
                                    if (temp_messages.Any(x => x.Value.ticket_edited_messages != string.Empty) == true)
                                    {
                                        logger.LogInformation("Tempmessages is not empty in HandleUpdate");
                                        try
                                        {
                                            foreach (var item in temp_messages)
                                            {
                                                if (item.Value.ticket_edited_messages == string.Empty)
                                                    continue;

                                                users.TryGetValue(item.Key, out var user);
                                                if (user != null)
                                                {
                                                    user.edited_message += item.Value.ticket_edited_messages + "\n";
                                                    item.Value.ticket_edited_messages = string.Empty;
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogWarning("Error happened in temp messages in main discord");
                                            logger.LogCritical(ex.Message);
                                        }

                                    }
                                    logger.LogInformation($"{DateTime.UtcNow} Adding message in HandleUpdate: {final}");
                                    val.edited_message += final + "\n";
                                }
                                else
                                {
                                    logger.LogInformation("Background is busy temp messages working in HandleUpdate");
                                    logger.LogInformation($"{DateTime.UtcNow} Adding message to temp edited: {final}");

                                    lock (BACKGROUND_ADD_MESSAGE_TO_DB.tempMessagesLock)
                                    {
                                        if (!temp_messages.TryGetValue(message.Channel.Id, out var val) || val == null)
                                        {
                                            val = new TicketLogs();
                                            temp_messages[message.Channel.Id] = val;
                                        }

                                        val.ticket_edited_messages += final + "\n";
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                logger.LogCritical("Unknown error in HandleUpdate adding to dict");
                                Console.WriteLine(ex);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogCritical("Unknown error in HandleUpdate ");
                    Console.WriteLine(ex);
                }
            }

        }

        private async Task HandleDelete(Cacheable<IMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2)
        {

            if (isDELdetectionOK)
            {
                try
                {
                    var channel = await cacheable2.GetOrDownloadAsync();
                    var message = await cacheable1.GetOrDownloadAsync();
                    if (message == null || message.Author == null)
                    {
                        logger.LogInformation("Author unknown in hadledelete");
                        return;
                    }
                    if (message.Author.IsBot)
                    {
                        logger.LogInformation("Author unknown is a bot in hadledelete");
                        return;
                    }
                    if (channel is ITextChannel chan)
                    {
                        if (!chan.CategoryId.HasValue)
                        {
                            logger.LogWarning("channel with no category in handledelete");
                            return;
                        }

                        if (chan.CategoryId == DISCORD_CRED.SUPPORT_CATEGORY)
                        {
                            var key = users.FirstOrDefault(x => x.Value.ticket_id == channel.Id).Key;
                            if (key == default)
                            {
                                logger.LogCritical($"{DateTime.UtcNow} No ticket with id {channel.Id} in dictionary users");
                                await ErrorMessageClass.ErrorMessageLogToUser($"No ticket with id {channel.Id} in dictionary users", channel.Id, _client);
                                return;
                            }

                            try
                            {
                                string final_cont = SafeString.Safe(message.Content) + $" {DateTime.UtcNow}";

                                if (BACKGROUND_ADD_MESSAGE_TO_DB.isBusy == false)
                                {
                                    users.TryGetValue(message.Channel.Id, out var val);
                                    if (val == null)
                                    {
                                        logger.LogCritical($"{DateTime.UtcNow} No value for  channel in dict {message.Channel.Id}");
                                        await ErrorMessageClass.ErrorMessageLogToUser($"No value for  channel in dict {message.Channel.Id}", message.Channel.Id, _client);
                                        return;
                                    }
                                    if (temp_messages.Any(x => x.Value.ticket_deleted_messages != string.Empty) == true)
                                    {
                                        logger.LogInformation("Tempmessages is not empty in HandleDelete");
                                        try
                                        {
                                            foreach (var item in temp_messages)
                                            {
                                                if (item.Value.ticket_deleted_messages == string.Empty)
                                                    continue;

                                                users.TryGetValue(item.Key, out var user);
                                                if (user != null)
                                                {
                                                    user.deleted_message += item.Value.ticket_deleted_messages + "\n";
                                                    item.Value.ticket_deleted_messages = string.Empty;
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogWarning("Error happened in HandleDeletes in main discord");
                                            logger.LogCritical(ex.Message);
                                        }

                                    }
                                    logger.LogInformation($"{DateTime.UtcNow} Adding message in HandleDelete: {final_cont}");
                                    val.deleted_message += final_cont + "\n";
                                }
                                else
                                {
                                    logger.LogInformation("Background is busy temp messages working in HandleDelete");
                                    logger.LogInformation($"{DateTime.UtcNow} Adding message to temp in HandleDelete : {final_cont}");

                                    lock (BACKGROUND_ADD_MESSAGE_TO_DB.tempMessagesLock)
                                    {
                                        if (!temp_messages.TryGetValue(message.Channel.Id, out var val) || val == null)
                                        {
                                            val = new TicketLogs();
                                            temp_messages[message.Channel.Id] = val;
                                        }

                                        val.ticket_deleted_messages += final_cont + "\n";
                                    }
                                }
                            }catch (Exception ex)
                            {
                                logger.LogCritical("Exception in adding deleted data to dic in HandleDelete");
                                Console.WriteLine(ex);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogCritical("Unknown error in HandleDelete main");
                    Console.WriteLine(ex);
                }
            }

        }

        private async Task HandleButton(SocketMessageComponent component)
        {
            if (component.Data.CustomId == "CL")
            {
                await component.DeferAsync();
                try
                {
                    logger.LogInformation("We are in Close ticket method");
                    users.TryGetValue(component.Channel.Id, out var user);
                    if (user == null)
                    {
                        logger.LogCritical($"No user in discionary with channel id {component.Channel.Id} CLOSE TICKET METHOD");
                        var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**Hm... Seems like we can`t find data about you.**\n" +
                        $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> if you believe an error occurred").Build();
                        await component.FollowupAsync(embed: emb);
                        return;
                    }
                    var embed = new EmbedBuilder().WithColor(Color.DarkOrange).WithTitle("Transcript").WithDescription($"<@{user.user_id}>, " +
                        $"do you want to receive a transcript of this ticket?").Build();
                    var butt = new ComponentBuilder().WithButton("Yes i need the transcript", "Y", ButtonStyle.Secondary)
                        .WithButton("No, thank you", "N", ButtonStyle.Secondary).Build();

                    await component.Message.ModifyAsync(x =>
                    {

                        x.Components = new ComponentBuilder().WithButton(label: "🔒 Close ticket", customId: "CL", ButtonStyle.Danger, disabled: true).Build();
                    });
                    await component.Channel.SendMessageAsync($"<@{user.user_id}>", embed: embed, components: butt);
                    logger.LogInformation($"Message was successfully sent and modified in CLOSE TICKET METHOD");
                }
                catch (Exception ex)
                {
                    logger.LogCritical("We are in Close ticket exception. Exception happened");
                    Console.WriteLine(ex);
                    var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**Hm... Seems like error happened**\n" +
                        $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> if you believe an error occurred\n\n " +
                        $"Exception message:\n\n {ex.Message}").Build();
                    await component.FollowupAsync(embed: emb);
                    return;
                }

            }
            if (component.Data.CustomId == "Y" || component.Data.CustomId == "N")
            {
                await component.DeferAsync();

                try
                {
                    logger.LogInformation("User clicked button Yes or No, we are in the start of the method");
                    users.TryGetValue(component.Channel.Id, out var user);
                    if (user == null)
                    {
                        logger.LogCritical($"No user in discionary with channel id {component.Channel.Id} YES OR NO METHOD");
                        var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**Hm... Seems like we can`t find data about you.**\n" +
                        $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> if you believe an error occurred").Build();
                        await component.FollowupAsync(embed: emb);
                        return;
                    }
                    var butt = new ComponentBuilder().WithButton("Yes i need the transcript", "Y", ButtonStyle.Secondary, disabled: true)
                        .WithButton("No, thank you", "N", ButtonStyle.Secondary, disabled: true).Build();
                    await component.Message.ModifyAsync(x =>
                    {

                        x.Components = butt;
                    });
                    Embed builder = null;
                    logger.LogInformation("Message sucessfully modified in YES OR NO method");
                    if (component.Data.CustomId == "Y")
                    {
                        builder = new EmbedBuilder().WithDescription($"<@{user.user_id}>, thank you for contacting us.\n" +
                        "The transcript will be sent to your DMs in 6 minutes. This ticket will be closed in 30 seconds.")
                            .WithColor(Color.Green)
                            .WithFooter("Make sure your DMs are open so the bot can send you your ticket transcript.")
                            .Build();

                    }
                    else
                    {
                        builder = new EmbedBuilder().WithDescription($"Thank you for contacting us. This ticket will be closed in 30 seconds.")
                            .WithColor(Color.Green).Build();
                    }
                    await component.Channel.SendMessageAsync(embed: builder);
                    logger.LogInformation($"Message was sucessfully sent to user to channel {component.ChannelId} in YES OR NO method");
                    _ = Task.Run(async () =>
                    {
                        logger.LogInformation("Async Task in YES OR NO method started");
                        var chanID = component.Channel.Id;
                        var channel = _client.GetChannel(chanID) as ITextChannel;
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        await channel.DeleteAsync();
                        var req2 = await _httpClient.PostAsync($"{REQ_URL.URL}/api/Home/SetStatus?secret={Secret.Secret_Key}&ticketID={chanID}", null);

                        await Task.Delay(TimeSpan.FromMinutes(6));
                        users.TryRemove(chanID, out var _);

                        if (component.Data.CustomId == "Y")
                        {
                            var req = await _httpClient.PostAsync($"{REQ_URL.URL}/api/Home/SendTicketHistory?secret={Secret.Secret_Key}&userID={user.user_id}" +
                                    $"&ticketID={chanID}", null);
                        }

                    });
                }
                catch (Exception ex)
                {
                    logger.LogCritical("Exception in YES OR NO METHOD");
                    Console.WriteLine(ex);
                    var emb = new EmbedBuilder().WithColor(Color.Red).WithDescription($"**Hm... Seems like error happened**\n" +
                        $"Please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}> if you believe an error occurred\n\n " +
                        $"Exception message:\n\n {ex.Message}").Build();
                    await component.FollowupAsync(embed: emb);
                    return;
                }
            }
        }

        private async Task HandleMenu(SocketMessageComponent component)
        {
            if (component.Data.CustomId.StartsWith($"Support - {GlobalID}"))
            {
                await component.DeferAsync(ephemeral: true);
                await component.FollowupAsync("Creating support ticket ...", ephemeral: true);
                _ = Task.Run(async () =>
                {
                    await ticketCreation.CreateTicket(component, _httpClient, _client);

                    GlobalID++;
                    string newCustomID = $"Support - {GlobalID}";

                    var menu = new SelectMenuBuilder()
                     .WithPlaceholder("Make support ticket")
                     .WithCustomId($"Support - {GlobalID}")
                     .WithMinValues(1)
                     .WithMaxValues(1)
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("General Questions")
                         .WithValue("GENERAL")
                         .WithDescription("Questions about the bot or server")
                         .WithEmote(new Emoji("❓")))
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("Bot Issues")
                         .WithValue("ISSUES")
                         .WithDescription("Report bot bugs or payment issues")
                         .WithEmote(new Emoji("🚨")))
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("Contact Admin")
                         .WithValue("ADMIN")
                         .WithDescription("Special requests, advices, or collaborations")
                         .WithEmote(new Emoji("🎓")))
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("Other")
                         .WithValue("OTHER")
                         .WithDescription("For questions not in other categories")
                         .WithEmote(new Emoji("🧾")));

                    var builder = new ComponentBuilder().WithSelectMenu(menu);

                    try
                    {
                        await component.Message.ModifyAsync(x =>
                        {
                            x.Components = builder.Build();

                        });
                        Console.WriteLine("Changed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine("Помилка при оновленні меню");
                        await component.FollowupAsync($"Hmmm... Something wrong with updating menu, " +
                            $"please contact a member of <@&{DISCORD_CRED.ROLE_FOR_TICKETPING}>", ephemeral: true);
                    }

                });
            }
        }

        private async Task ReadyAsync()
        {
            logger.LogInformation("Bot is ready");

            try
            {
                var channel = await _client.GetChannelAsync(DISCORD_CRED.CHANNEL_FOR_STARTMESSAGE) as IMessageChannel;
                if (channel != null)
                {
                    var embed = new EmbedBuilder()
                            .WithTitle("📬 Support Center")
                            .WithColor(Color.Green)
                            .WithDescription(
                                "**Hello!** 👋\n" +
                                "You're now in the **Support Center** — the place where you can get help on anything related to the server or the bot.\n\n" +

                                "### 📝 Available Support Types:\n" +
                                "> • **General Questions** — anything you're unsure about\n" +
                                "> • **Сontact Admin** — some special cases or personal questions\n" +
                                "> • **Bot Issues** — problems or bugs\n" +
                                "> • **Other Requests** — everything that doesn't fit above\n\n" +

                                "**Select a category below to open a ticket.**\n" +
                                "Our staff will respond as soon as possible! 💬"
                            )
                            .Build();

                    var menu = new SelectMenuBuilder()
                     .WithPlaceholder("Make support ticket")
                     .WithCustomId($"Support - {GlobalID}")
                     .WithMinValues(1)
                     .WithMaxValues(1)
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("General Questions")
                         .WithValue("GENERAL")
                         .WithDescription("Questions about the bot or server")
                         .WithEmote(new Emoji("❓")))
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("Bot Issues")
                         .WithValue("ISSUES")
                         .WithDescription("Report bot bugs or payment issues")
                         .WithEmote(new Emoji("🚨")))
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("Contact Admin")
                         .WithValue("ADMIN")
                         .WithDescription("Special requests, advices, or collaborations")
                         .WithEmote(new Emoji("🎓")))
                     .AddOption(new SelectMenuOptionBuilder()
                         .WithLabel("Other")
                         .WithValue("OTHER")
                         .WithDescription("For questions not in other categories")
                         .WithEmote(new Emoji("🧾")));

                    var builder = new ComponentBuilder().WithSelectMenu(menu);

                    var sentMessage = await channel.SendMessageAsync(embed: embed, components: builder.Build());

                    var messages = await channel.GetMessagesAsync(50).FlattenAsync();
                    foreach (var msg in messages)
                    {
                        if (msg.Id != sentMessage.Id)
                        {
                            try
                            {
                                await msg.DeleteAsync();
                            }
                            catch
                            {

                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
