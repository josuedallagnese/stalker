using Stalker.Helpers;
using Stalker.Watchers;

namespace Stalker.Configuration
{
    public class StalkerConfiguration
    {
        public string DashboardPath { get; set; }
        public string Username { get; set; }
        public string Paswword { get; set; }
        public SlackConfiguration Slack { get; set; }
        public TeamsConfiguration Teams { get; set; }
        public List<EnvironmentConfiguration> Environments { get; set; }
        public List<StalkerGroupConfiguration> Groups { get; set; }

        public WatcherExecutionContext CreateExecutionContext(string environmentId, string groupId)
        {
            var environment = Environments.SingleOrDefault(s => s.Id == environmentId);
            var group = GetGroup(groupId);
            var requestPathParameters = GetRequestPathParameters(group.Watchers);
            var requestBodyParameters = GetRequestBodyParameters(group.Watchers);

            return new WatcherExecutionContext(
                environment,
                group,
                requestPathParameters,
                requestBodyParameters);
        }

        public IEnumerable<StalkerGroupConfiguration> GetGroupsToRegister(bool hasEnvironment)
        {
            return Groups
                .Where(w => w.Watchers.Any(a => EnvironmentHelper.HasEnvironment(a.Http.Url) == hasEnvironment))
                .ToArray();
        }

        private StalkerGroupConfiguration GetGroup(string groupId)
        {
            var group = Groups.Single(s => s.Id == groupId);
            return group;
        }

        private Dictionary<string, string> GetRequestPathParameters(IEnumerable<WatcherConfiguration> watchers)
        {
            var parameters = new Dictionary<string, string>();

            foreach (var watcher in watchers.Where(w => w.IsHttp))
            {
                var url = watcher.Http.Url.Replace("{{environment}}", string.Empty);

                var result = url.Split(new string[] { "{{", "}}" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                result.RemoveAll(w => w.Contains("/"));
                result.RemoveAll(w => w.Contains("//"));

                foreach (var items in result.Chunk(2))
                {
                    parameters.Add($"{{{{{items.ElementAt(0)}}}}}{items.ElementAt(1)}", null);
                }
            }

            return parameters;
        }

        private Dictionary<string, string> GetRequestBodyParameters(IEnumerable<WatcherConfiguration> watchers)
        {
            var parameters = new Dictionary<string, string>();

            foreach (var watcher in watchers.Where(w => w.IsHttp).Where(w => w.Http.Content != null))
            {
                foreach (var values in watcher.Http.Content.Values)
                {
                    var value = values.ToString();

                    if (value.StartsWith("{{"))
                    {
                        parameters.Add(value, null);
                    }
                }
            }

            return parameters;
        }
    }
}
