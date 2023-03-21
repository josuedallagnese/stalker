using Hangfire;
using Hangfire.MemoryStorage;
using Stalker.Configuration;
using Stalker.Watchers;
using Stalker.Notifiers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HangfireBasicAuthenticationFilter;
using Hangfire.Dashboard;
using Stalker.Watchers.Internal;
using Stalker.Notifiers.Internal;

var builder = WebApplication.CreateBuilder(args);

var configurationFile = builder.Configuration.GetValue<string>("ConfigurationFile");

if (string.IsNullOrWhiteSpace(configurationFile))
    throw new InvalidProgramException("ConfigurationFile property is not defined");
else if (!configurationFile.IsUrl() && !File.Exists(configurationFile))
    throw new InvalidProgramException("ConfigurationFile property is not defined");

StalkerConfiguration stalkerConfiguration = null;

var jsonSerializerOptions = new JsonSerializerOptions();

jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

if (configurationFile.IsUrl())
{
    using var client = new HttpClient();

    var httpResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, configurationFile));

    var configurationFileContent = await httpResponse.Content.ReadAsStringAsync();

    if (!httpResponse.IsSuccessStatusCode)
        throw new InvalidProgramException($"Configuration file cannot be loaded. Cause: {configurationFileContent}");

    stalkerConfiguration = JsonSerializer.Deserialize<StalkerConfiguration>(configurationFileContent, jsonSerializerOptions);
}
else
{
    var configurationFileContent = File.ReadAllText(configurationFile, Encoding.UTF8);

    stalkerConfiguration = JsonSerializer.Deserialize<StalkerConfiguration>(configurationFileContent, jsonSerializerOptions);
}

if (stalkerConfiguration == null)
    throw new InvalidProgramException("Invalid configuration file content");

builder.Services.AddSingleton(stalkerConfiguration);

builder.Services.AddLogging(configure => configure.AddConsole());

builder.Services.AddSingleton<IWatcherOperationCollector, WatcherOperationCollector>();
builder.Services.AddSingleton<INotifier, SlackNotifier>();
builder.Services.AddSingleton<INotifier, TeamsNotifier>();
builder.Services.AddScoped<HttpWatcher>();
builder.Services.AddHttpClient<HttpWatcher>();

builder.Services.AddHangfire(c => c.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

foreach (var group in stalkerConfiguration.GetGroupsToRegister(hasEnvironment: true))
{
    foreach (var environment in stalkerConfiguration.Environments)
    {
        var jobId = $"{environment.Id} - {group.Id}";

        jobManager.AddOrUpdate<HttpWatcher>(
            jobId,
            job => job.ExecuteAsync(environment.Id, group.Id),
            group.Cron);

        logger.LogInformation($"Job {jobId} registed.");
    }
}

foreach (var group in stalkerConfiguration.GetGroupsToRegister(hasEnvironment: false))
{
    var jobId = group.Id;

    jobManager.AddOrUpdate<HttpWatcher>(
    jobId,
        job => job.ExecuteAsync(null, group.Id),
        group.Cron);

    logger.LogInformation($"Job {jobId} registed.");
}

logger.LogInformation($"Dashboard is active on {stalkerConfiguration.DashboardPath}.");

app.UseHangfireDashboard(stalkerConfiguration.DashboardPath, new DashboardOptions
{
    Authorization = new List<IDashboardAuthorizationFilter>()
    {
        new HangfireCustomBasicAuthenticationFilter()
        {
            User = stalkerConfiguration.Username,
            Pass = stalkerConfiguration.Paswword
        }
    }
});

app.Run();
