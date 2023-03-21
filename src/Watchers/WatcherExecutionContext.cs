using System.Text;
using System.Text.Json;
using Stalker.Configuration;
using Stalker.Helpers;

namespace Stalker.Watchers
{
    public class WatcherExecutionContext
    {
        private const string InitialTagMark = "{{";
        private const string FinalTagMark = "}}";

        public EnvironmentConfiguration Environment { get; }
        public StalkerGroupConfiguration Group { get; }
        public IDictionary<string, string> RequestPathParameters { get; }
        public IDictionary<string, string> RequestBodyParameters { get; }
        public IDictionary<string, OperationResult> OperationResults { get; }

        public WatcherExecutionContext(
            EnvironmentConfiguration environment,
            StalkerGroupConfiguration group,
            IDictionary<string, string> requestPathParameters,
            IDictionary<string, string> requestBodyParameters)
        {
            Environment = environment;
            Group = group;
            RequestPathParameters = requestPathParameters;
            RequestBodyParameters = requestBodyParameters;

            OperationResults = new Dictionary<string, OperationResult>();
        }

        public OperationResult CreateOperation(WatcherConfiguration watcher)
        {
            var url = GetWatcherUrl(watcher);

            if (OperationResults.ContainsKey(url))
                return OperationResults[url];

            var operation = new OperationResult(url);

            OperationResults.Add(url, operation);

            return operation;
        }

        public bool HasEnvironment() => Environment != null;

        public string GetWatcherUrl(WatcherConfiguration watcher)
        {
            var url = EnvironmentHelper.GetEnvironmentUrl(watcher.Http.Url, Environment);

            foreach (var param in RequestPathParameters.Where(w => w.Value != null))
            {
                url = JsonHelper.Replace(url, param.Key, param.Value);
            }

            return url;
        }

        public void PrepareRequest(WatcherConfiguration watcher, HttpRequestMessage httpRequest)
        {
            if (watcher.Http.Type == RequestType.Json)
            {
                if (watcher.Http.Content == null)
                    return;

                var content = JsonSerializer.Serialize(watcher.Http.Content);

                foreach (var param in RequestBodyParameters.Where(w => w.Value != null))
                {
                    if (watcher.Http.Headers != null)
                    {
                        foreach (var header in watcher.Http.Headers)
                        {
                            if (header.Value.Contains(param.Key))
                                httpRequest.Headers.Add(header.Key, JsonHelper.Replace(header.Value, param.Key, param.Value));
                        }
                    }

                    if (content.Contains(param.Key))
                        content = JsonHelper.Replace(content, param.Key, param.Value);
                }

                httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
        }

        public async Task PrepareResponse(WatcherConfiguration watcher, HttpResponseMessage httpResponse)
        {
            var success = HadSuccess(watcher, httpResponse);
            var statusCode = (int)httpResponse.StatusCode;
            var content = await GetResponseContent(httpResponse);

            GetOperationResult(watcher)
                .WithResult(success, statusCode, content);

            if (!httpResponse.IsSuccessStatusCode || !JsonHelper.IsJsonResult(content))
                return;

            var document = JsonDocument.Parse(content);

            var parametersPathFound = RequestPathParameters.Keys.Where(w => w.IndexOf(InitialTagMark) > -1);

            foreach (var keyFound in parametersPathFound)
            {
                var propertyToExtract = ExtractParameterName(keyFound);
                string value = null;

                if (JsonHelper.IsJsonArray(propertyToExtract))
                    value = JsonHelper.ReadArrayProperty(document.RootElement, propertyToExtract);
                else
                    value = document.RootElement.GetProperty(propertyToExtract).GetRawText();

                RequestPathParameters[keyFound] = JsonHelper.Replace(value, "\"", "");
            }

            var parametersBodyFound = RequestBodyParameters.Keys.Where(w => w.IndexOf(InitialTagMark) > -1);

            foreach (var keyFound in parametersBodyFound)
            {
                var propertyToExtract = ExtractParameterName(keyFound);
                string value = null;

                if (JsonHelper.IsJsonArray(propertyToExtract))
                    value = JsonHelper.ReadArrayProperty(document.RootElement, propertyToExtract);
                else
                    value = document.RootElement.GetProperty(propertyToExtract).GetRawText();

                RequestBodyParameters[keyFound] = JsonHelper.Replace(value, "\"", "");
            }
        }

        public OperationResult GetOperationResult(WatcherConfiguration watcher)
        {
            var url = GetWatcherUrl(watcher);
            return OperationResults[url];
        }

        public HealthyStatus GetHealthyStatus() => OperationResults.Values.All(a => a.Success) ? HealthyStatus.Healthy : HealthyStatus.Unhealthy;

        public string GetEnvironmentId()
        {
            if (HasEnvironment())
                return Environment.Id;

            return Group.Id;
        }


        private bool HadSuccess(WatcherConfiguration watcher, HttpResponseMessage httpResponse)
        {
            if (watcher.Http.ShouldBe == null)
                return watcher.Http.ShouldBe.StatusCode == 200;
            else
                return watcher.Http.ShouldBe.StatusCode == (int)httpResponse.StatusCode;
        }

        private async Task<string> GetResponseContent(HttpResponseMessage httpResponse)
        {
            var statusCode = (int)httpResponse.StatusCode;

            var content = await httpResponse.Content.ReadAsStringAsync();

            if (statusCode == 404 || statusCode == 503)
                content = null;

            return content;
        }

        private static string ExtractParameterName(string keyFound)
        {
            return keyFound.Replace(InitialTagMark, string.Empty).Replace(FinalTagMark, string.Empty);
        }
    }
}
