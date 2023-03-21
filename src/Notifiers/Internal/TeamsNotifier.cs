using Stalker.Configuration;
using Stalker.Watchers;

namespace Stalker.Notifiers.Internal
{
    internal class TeamsNotifier : INotifier
    {
        private readonly StalkerConfiguration _configuration;

        public TeamsNotifier(StalkerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task NotifyAsync(WatcherExecutionContext context)
        {
            if (_configuration.Teams == null)
                return Task.CompletedTask;

            return Task.CompletedTask;
        }
    }
}
