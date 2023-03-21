using System.Collections.Concurrent;
using Stalker.Notifiers;

namespace Stalker.Watchers.Internal
{
    internal class WatcherOperationCollector : IWatcherOperationCollector
    {
        private static readonly ConcurrentDictionary<string, OperationCollectorResult> _data = new();
        private readonly IEnumerable<INotifier> _notifiers;

        public WatcherOperationCollector(IEnumerable<INotifier> notifiers)
        {
            _notifiers = notifiers;
        }

        public async Task Collect(WatcherExecutionContext context)
        {
            var collectorResult = _data.GetOrAdd(context.Group.Id, (groupId) =>
            {
                return new OperationCollectorResult()
                {
                    GroupId = context.Group.Id
                };
            });

            collectorResult.CollectResult(context);

            if (collectorResult.ShouldNotify)
            {
                foreach (var notifier in _notifiers)
                {
                    await notifier.NotifyAsync(context);
                }
            }
        }

        public OperationCollectorResult GetStatistic(string groupId)
        {
            _data.TryGetValue(groupId, out var result);

            return result;
        }

        public IEnumerable<OperationCollectorResult> GetStatistics() => _data.Values.ToArray();
    }
}
