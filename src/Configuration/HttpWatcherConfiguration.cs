namespace Stalker.Configuration
{
    public class HttpWatcherConfiguration
    {
        public RequestType Type { get; set; }
        public string HttpMethod { get; set; }
        public string Url { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, object> Content { get; set; }
        public HttpResponseShouldBe ShouldBe { get; set; }

        public class HttpResponseShouldBe
        {
            public int StatusCode { get; set; }
        }
    }
}
