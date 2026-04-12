namespace Support_Bot.DB_INTERFACES
{
    public interface ISetStatus
    {
        public Task<bool> SetStat(string ticket_id);
    }
}
