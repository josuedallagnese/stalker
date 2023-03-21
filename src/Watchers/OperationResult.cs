namespace Stalker.Watchers
{
    public class OperationResult
    {
        public string Url { get; }
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string ServiceResult { get; private set; }

        public OperationResult(string url)
        {
            Url = url;
        }

        public override string ToString()
        {
            return $"[Success {Success}]: {StatusCode} - {Url}";
        }

        public void RegisterException(HttpRequestException ex)
        {
            if (ex.StatusCode.HasValue)
                StatusCode = (int)ex.StatusCode;

            RegisterException(ex);
        }

        public void RegisterException(Exception ex)
        {
            ServiceResult = ex.Message;
            Success = false;
        }

        public void RegisterTimeout(double totalSeconds)
        {
            ServiceResult = $"The request was canceled due to the configured timeout of {totalSeconds} seconds.";
            Success = false;
        }

        public void WithResult(bool success, int statusCode, string serviceResult)
        {
            Success = success;
            StatusCode = statusCode;

            if (!success)
                ServiceResult = serviceResult;
        }
    }
}
