namespace Stalker.Watchers
{
    public interface IWatcherOperationCollector
    {
        OperationCollectorResult GetStatistic(string groupId);
        IEnumerable<OperationCollectorResult> GetStatistics();
        Task Collect(WatcherExecutionContext context);
    }
}
