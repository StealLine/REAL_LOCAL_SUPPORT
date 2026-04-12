namespace Support_Bot.DB_INTERFACES
{
    public interface IDBaddDeletedMessage
    {
        public Task<bool> AddDel(string id, string deleted_Message);
    }
}
