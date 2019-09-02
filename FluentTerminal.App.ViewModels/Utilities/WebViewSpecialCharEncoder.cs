namespace FluentTerminal.App.ViewModels.Utilities
{
    public static class WebViewSpecialCharEncoder
    {
        public static string Encode(string value)
        {
            return value.Replace("\"", "&quot;").Replace("'", "&squo;").Replace("\\", "&bsol;");
        }

        public static string Decode(string value)
        {
            return value.Replace("&quot;", "\"").Replace("&squo;", "'").Replace("&bsol;", "\\");
        }
    }
}
