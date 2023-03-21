namespace Stalker.Configuration
{
    public class WatcherConfiguration
    {
        public HttpWatcherConfiguration Http { get; set; }

        public bool IsHttp => Http != null;

        public override string ToString()
        {
            if (IsHttp) return $"{Http.HttpMethod} - {Http.Url}";

            return base.ToString();
        }
    }
}
