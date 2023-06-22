using DataCollectors.OPCUA.Core.Application.HostedServices;
using DataCollectors.OPCUA.Core.Application.Shared.Services;
using DataCollectors.OPCUA.Core.Application.UaClient.Factories;
using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace DataCollectors.OPCUA.Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);

                config.NotificationPublisher = new TaskWhenAllPublisher();
                config.NotificationPublisherType = typeof(TaskWhenAllPublisher);
                config.Lifetime = ServiceLifetime.Transient;
            })
            .AddCustomServices();

        return services;
    }

    private static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .Scan(scan => scan
                .FromAssemblyOf<AlarmEventService>()
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                .AsMatchingInterface()
                .WithScopedLifetime());

        services
            .Scan(scan => scan
                .FromAssemblyOf<AlarmEventService>()
                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                .AsMatchingInterface()
                .WithTransientLifetime());

        services
            .AddHostedService<AlarmEventService>();

        services
            .AddSingleton<IUaClientService, UaClientService>()
            .AddSingleton<IClientFactory, ClientFactory>();

        return services;
    }
}