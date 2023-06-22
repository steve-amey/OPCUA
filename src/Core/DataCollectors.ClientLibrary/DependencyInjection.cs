using DataCollectors.ClientLibrary.Behaviours;
using DataCollectors.ClientLibrary.Constants;
using DataCollectors.ClientLibrary.Factories.Logging;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services;
using DataCollectors.ClientLibrary.Services.Background;
using DataCollectors.ClientLibrary.Services.Http;
using DataCollectors.ClientLibrary.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace DataCollectors.ClientLibrary;

public static class DependencyInjection
{
    public const string ConfigurationServiceHttpClient = "ConfigurationService";
    public const string WebApiHttpClient = "WebApi";

    public static IServiceCollection AddClientLibraryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediatRPipelineBehaviours()
            .AddValidatorsFromAssembly(typeof(BaseValidator<>).Assembly);

        services
            .AddHostedService<SchedulerService>()
            .AddSingleton<IHostedServiceManager, SchedulerService>();

        services
            .AddSingleton<ICustomLoggerFactory, CustomLoggerFactory>()
            .AddCustomServices();

        services
            .AddHttpClient(WebApiHttpClient, (_, _) =>
            {
            })
            .AddPolicyHandler(RetryPolicySelector);

        services
            .AddHttpClient(ConfigurationServiceHttpClient, (provider, client) =>
            {
                var appSettings = provider.GetRequiredService<IOptions<AppSettings>>().Value;
                var configurationWebApi = appSettings.WebApi.Single(x => x.Name == Contracts.Enums.WebApiNames.Thingworx);

                client.BaseAddress = new Uri(configurationWebApi.BaseUrl);
            });

        return services;
    }

    private static IServiceCollection AddMediatRPipelineBehaviours(this IServiceCollection services)
    {
        services
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    private static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .Scan(scan => scan
                .FromAssemblyOf<IScopedService>()
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                .AsMatchingInterface()
                .WithScopedLifetime());

        services
            .Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                .AsMatchingInterface()
                .WithTransientLifetime());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> RetryPolicySelector(IServiceProvider provider, HttpRequestMessage request)
    {
        var appSettings = provider.GetRequiredService<IOptions<AppSettings>>();

        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: appSettings.Value.Retry);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay, (result, timespan, retryAttempt, context) =>
            {
                var logger = CreateLoggerForWebApi(request.RequestUri!, appSettings.Value, provider);

                if (retryAttempt == 1)
                {
                    if (result.Exception != null)
                    {
                        logger
                            .LogError(result.Exception.Message);
                    }
                    else
                    {
                        string? error = null;

                        try
                        {
                            error = result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            logger
                                .LogError("Error performing ReadAsStringAsync on HttpRequestMessage: {error}", ex.Message);
                        }

                        if (string.IsNullOrWhiteSpace(error))
                        {
                            error = $"{result.Result.StatusCode} - {result.Result.ReasonPhrase}";
                        }

                        logger
                            .LogError(error);
                    }
                }

                logger
                    .LogWarning("Delaying for {delay}ms, then making retry {retry}.", timespan.TotalMilliseconds, retryAttempt);
            });

        return retryPolicy;
    }

    private static ILogger CreateLoggerForWebApi(Uri requestMessage, AppSettings appSettings, IServiceProvider provider)
    {
        var baseUrl = $"{requestMessage.Scheme}://{requestMessage.Host}";

        var webApi = appSettings.WebApi.SingleOrDefault(x => x.BaseUrl.StartsWith(baseUrl));

        var loggerFactory = provider.GetRequiredService<ICustomLoggerFactory>();
        var logger = loggerFactory.CreateLogger<AlarmHttpService>(webApi);

        return logger;
    }
}