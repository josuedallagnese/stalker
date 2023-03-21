using Stalker.Configuration;

namespace Stalker.Watchers
{
    public interface IWatcher
    {
        WatcherType Type { get; }
        Task ExecuteAsync(string environmentId, string id);
    }
}
