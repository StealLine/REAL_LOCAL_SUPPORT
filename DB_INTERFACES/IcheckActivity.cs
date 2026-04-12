namespace Support_Bot.DB_INTERFACES
{
    public interface IcheckActivity
    {
        public Task<string> Check(string userID);
    }
}
