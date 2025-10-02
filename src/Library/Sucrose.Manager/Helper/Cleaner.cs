namespace Sucrose.Manager.Helper
{
    internal static class Cleaner
    {
        public static string Clean(string Content)
        {
            try
            {
                if (Content.StartsWith("{{"))
                {
                    Content = Content[1..];
                }

                if (Content.EndsWith("}}"))
                {
                    Content = Content[..^1];
                }

                if (Content.StartsWith("{{") || Content.EndsWith("}}"))
                {
                    return Clean(Content);
                }
                else
                {
                    return Content;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}