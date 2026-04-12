namespace Support_Bot.SENDTXT
{
    public interface IdiscordGetTXT
    {
        public Task<string> GetTXT(string ticketID);
    }
}
