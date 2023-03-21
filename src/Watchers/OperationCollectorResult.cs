namespace Stalker.Watchers
{
    public class OperationCollectorResult
    {
        public string GroupId { get; set; }
        public HealthyStatus Status { get; set; }
        public bool? CurrentAttempt { get; set; }
        public bool ShouldNotify { get; set; }

        public void CollectResult(WatcherExecutionContext context)
        {
            Status = context.GetHealthyStatus();

            var lastAttempt = CurrentAttempt;

            if (Status == HealthyStatus.Healthy)
                CurrentAttempt = true;
            else
                CurrentAttempt = false;

            if (lastAttempt.HasValue)
            {
                if (!lastAttempt.Value && CurrentAttempt.Value)
                    ShouldNotify = true;
            }

            if (!CurrentAttempt.Value)
                ShouldNotify = true;
        }
    }
}
