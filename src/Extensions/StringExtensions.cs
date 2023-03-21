using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        public static bool IsUrl(this string value) => value.StartsWith("http");
    }
}
