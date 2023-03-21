using System.Text.Json;

namespace Stalker.Helpers
{
    public static class JsonHelper
    {
        public static bool IsJsonResult(string content) => IsJsonObject(content) || IsJsonArray(content);
        public static bool IsJsonObject(string content) => content.StartsWith('{') && content.StartsWith('}');
        public static bool IsJsonArray(string content) => content.Contains('[') && content.Contains(']');

        public static string ReadArrayProperty(JsonElement element, string property)
        {
            var arrayProperty = property.Substring(0, property.IndexOf("["));
            var arrayIndex = property.Split('[', ']')[1];

            var subElement = element.GetProperty(arrayProperty).EnumerateArray().ElementAt(int.Parse(arrayIndex));

            property = property.Substring(property.IndexOf("].") + 2);

            if (JsonHelper.IsJsonArray(property))
            {
                return ReadArrayProperty(subElement, property);
            }

            return Replace(subElement.GetProperty(property).GetRawText(), "\"", "");
        }

        public static string Replace(string content, string key, string value)
        {
            content = content.Replace(key, value);

            if (content.Contains(key))
            {
                content = Replace(content, key, value);
            }

            return content;
        }
    }
}
