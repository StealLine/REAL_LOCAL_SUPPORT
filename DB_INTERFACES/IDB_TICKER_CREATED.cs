namespace Support_Bot.DB_INTERFACES
{
    public interface IDB_TICKER_CREATED
    {
        public Task<string> DB_ADD(string creatorID, string ticketID, string ticket_type);
    }
}
