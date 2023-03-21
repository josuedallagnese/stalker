using Stalker.Configuration;

namespace Stalker.Helpers
{
    public static class EnvironmentHelper
    {
        public const string Tag = "{{environment}}";

        public static bool HasEnvironment(string url) => url.Contains(Tag);

        public static string GetEnvironmentUrl(string url, EnvironmentConfiguration environment)
        {
            if (environment == null)
                return url;

            if (!HasEnvironment(url))
                return url;

            return url.Replace(Tag, environment.Url);
        }
    }
}
