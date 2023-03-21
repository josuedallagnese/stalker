namespace Stalker.Configuration
{
    public class StalkerGroupConfiguration
    {
        public string Id { get; set; }
        public string Cron { get; set; }
        public bool AlwaysNotify { get; set; }
        public List<WatcherConfiguration> Watchers { get; set; }
    }
}
