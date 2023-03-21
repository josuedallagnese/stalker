using Stalker.Configuration;
using System.ComponentModel;

namespace Stalker.Watchers.Internal
{
    internal class HttpWatcher : IWatcher
    {
        private readonly HttpClient _client;
        private readonly StalkerConfiguration _stalkerConfiguration;
        private readonly IWatcherOperationCollector _operationCollector;
        private readonly ILogger _logger;

        public WatcherType Type => WatcherType.Http;

        public HttpWatcher(HttpClient client, StalkerConfiguration stalkerConfiguration, IWatcherOperationCollector operationCollector, ILogger<HttpWatcher> logger)
        {
            _client = client;
            _stalkerConfiguration = stalkerConfiguration;
            _operationCollector = operationCollector;
            _logger = logger;
        }

        [DisplayName("{0} {1}")]
        public async Task ExecuteAsync(string environmentId, string groupId)
        {
            var context = _stalkerConfiguration.CreateExecutionContext(environmentId, groupId);

            environmentId = context.GetEnvironmentId();

            if (context.HasEnvironment())
                _client.Timeout = context.Environment.Timeout;

            _logger.LogInformation($"Starting verification to {environmentId}");

            foreach (var watcher in context.Group.Watchers)
            {
                var operationResult = await WatchAsync(context, watcher);

                if (!operationResult.Success)
                    break;
            }

            await _operationCollector.Collect(context);

            var healthy = context.GetHealthyStatus();

            _logger.LogInformation($"{environmentId} is {healthy}");
        }

        private async Task<OperationResult> WatchAsync(WatcherExecutionContext context, WatcherConfiguration watcher)
        {
            var operation = context.CreateOperation(watcher);

            try
            {
                var url = operation.Url;

                if (watcher.Http.Timeout.HasValue)
                    _client.Timeout = watcher.Http.Timeout.Value;

                var method = new HttpMethod(watcher.Http.HttpMethod);

                var httpRequest = new HttpRequestMessage(method, url);

                context.PrepareRequest(watcher, httpRequest);

                var httpResponse = await _client.SendAsync(httpRequest);

                await context.PrepareResponse(watcher, httpResponse);
            }
            catch (HttpRequestException ex)
            {
                operation.RegisterException(ex);

                _logger.LogError(ex, operation.ServiceResult);
            }
            catch (TaskCanceledException ex)
            {
                operation.RegisterTimeout(_client.Timeout.TotalSeconds);

                _logger.LogError(ex, operation.ServiceResult);
            }
            catch (Exception ex)
            {
                operation.RegisterException(ex);

                _logger.LogError(ex, operation.ServiceResult);
            }

            return operation;
        }
    }
}
