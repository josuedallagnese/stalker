using Stalker.Watchers;

namespace Stalker.Notifiers
{
    public interface INotifier
    {
        Task NotifyAsync(WatcherExecutionContext context);
    }
}
