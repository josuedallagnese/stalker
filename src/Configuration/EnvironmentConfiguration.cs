namespace Stalker.Configuration
{
    public class EnvironmentConfiguration
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public TimeSpan Timeout { get; set; }

        public override string ToString() => $"{Id} - {Url}";
    }
}
