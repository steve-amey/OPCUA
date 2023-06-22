using System.Diagnostics;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.Alarm;
using DataCollectors.ClientLibrary.Services.Background.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataCollectors.ClientLibrary.Services.Background;

internal class SchedulerService : BackgroundService, IHostedServiceManager
{
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SchedulerService(IOptions<AppSettings> appSettings, ILogger<SchedulerService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public BackgroundServiceStatus Status { get; private set; }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started at: {time}", DateTimeOffset.Now.ToString("G"));

        Status = BackgroundServiceStatus.Running;

        try
        {
            await SendToWebApis();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending to web apis on startup.");
        }

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();

        _logger.LogInformation("Stopped at: {time}", DateTimeOffset.Now.ToString("G"));

        await base.StopAsync(cancellationToken);

        Status = BackgroundServiceStatus.Stopped;

        _logger.LogInformation("Service took {ms} ms to stop.", stopWatch.ElapsedMilliseconds);
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        if (Status is BackgroundServiceStatus.Stopped or BackgroundServiceStatus.StopRequested)
        {
            await StartAsync(cancellationToken);
        }
    }

    public Task RequestStop()
    {
        Status = BackgroundServiceStatus.StopRequested;
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var timer = new PeriodicTimer(_appSettings.ScheduleInterval);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    if (Status == BackgroundServiceStatus.StopRequested)
                    {
                        await StopAsync(stoppingToken);
                        return;
                    }

                    Status = BackgroundServiceStatus.Executing;

                    _logger.LogInformation("*** Running. Next run at: {time} ***", DateTime.Now.Add(_appSettings.ScheduleInterval).ToString("G"));

                    await SendToWebApis();

                    Status = BackgroundServiceStatus.Running;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aggregateException)
                {
                    foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                    {
                        _logger.LogError(innerException, "One or many tasks failed.");
                    }
                }
                else
                {
                    _logger.LogError(ex, "Error sending to web apis.");
                }
            }
        }
    }

    private Task SendToWebApis()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();

        _ = alarmService
            .SendStoredItems();

        return Task.CompletedTask;
    }
}
