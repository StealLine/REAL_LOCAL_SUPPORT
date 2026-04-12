namespace Support_Bot.DB_INTERFACES
{
    public interface IDBAddEditedMessage
    {
        public Task<bool> AddEditedM(string id, string original, string edited);
    }
}
