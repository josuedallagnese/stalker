using Microsoft.AspNetCore.WebUtilities;

namespace Stalker.Extensions
{
    public static class HttpExtensions
    {
        public static Uri AddQueryString(this Uri uri, string name, string value)
        {
            var finalUri = QueryHelpers.AddQueryString(uri.ToString(), name, value);

            return new Uri(finalUri);
        }

        public static Uri ReplacePathParameter(this Uri uri, string name, string value)
        {
            var url = uri.ToString().Replace($"{{{name}}}", value);

            return new Uri(url);
        }
    }
}
