using SlackBotMessages;
using SlackBotMessages.Models;
using Stalker.Configuration;
using Stalker.Watchers;

namespace Stalker.Notifiers.Internal
{
    internal class SlackNotifier : INotifier
    {
        private readonly StalkerConfiguration _configuration;

        public SlackNotifier(StalkerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task NotifyAsync(WatcherExecutionContext context)
        {
            if (_configuration.Slack == null)
                return;

            var client = new SbmClient(_configuration.Slack.WebHookUrl);
            var id = context.GetEnvironmentId();
            var healthy = context.GetHealthyStatus();

            var message = new Message($"*{healthy}*: _{id}_")
                .SetUserWithEmoji(_configuration.Slack.Username, Emoji.Alien);

            foreach (var operation in context.OperationResults)
            {
                var attachment = new Attachment()
                    .AddField("Service", operation.Value.Url, false)
                    .AddField("Status Code", operation.Value.StatusCode.ToString(), true);

                attachment.Color = "good";

                if (!operation.Value.Success)
                {
                    if (!string.IsNullOrWhiteSpace(operation.Value.ServiceResult))
                        attachment.AddField("Service Result", operation.Value.ServiceResult, false);

                    attachment.Color = "danger";
                }

                message.AddAttachment(attachment);
            }

            await client.SendAsync(message);
        }
    }
}
