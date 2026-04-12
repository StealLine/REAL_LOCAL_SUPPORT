namespace Support_Bot.DB_METHODS.Entitys
{
    public class TicketLogs
    {
        public int Id { get; set; }
        public DateTime time_created { get; set; }
        public DateTime time_closed { get; set; }
        public string ticket_content { get; set; }
        public string ticket_deleted_messages { get; set; }
        public string ticket_edited_messages { get; set; }
        public string ticket_creator_id {  get; set; }
        public string ticket_type { get; set; }
        public string ticket_id { get; set; }
        public bool isactive { get; set; }

    }
}
