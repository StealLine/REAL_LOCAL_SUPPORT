namespace Support_Bot.DISCORDBOT.DISCORDMODELS
{
    public class USER_MODEL
    {
        public ulong user_id {  get; set; }
        public ulong ticket_id { get; set; }
        public string ticket_type {  get; set; }
        public string edited_message { get; set; }
        public string deleted_message { get; set; }
        public string message_history { get; set; }

    }
}
