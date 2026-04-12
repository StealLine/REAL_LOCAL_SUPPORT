namespace Support_Bot.MANAGEMENT_CLASSES
{
    public static class SafeString
    {
        public static string Safe(string input)
        {
            return input
            .Replace("\\", "\\\\")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n")
            .Replace("\r", "\\n")
            .Replace("\t", "\\t")
            .Replace("\f", "\\f")
            .Replace("\v", "\\v");
        }
    }
}
