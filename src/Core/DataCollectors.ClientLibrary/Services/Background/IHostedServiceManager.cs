using DataCollectors.ClientLibrary.Services.Background.Enums;

namespace DataCollectors.ClientLibrary.Services.Background;

public interface IHostedServiceManager
{
    BackgroundServiceStatus Status { get; }

    Task Start(CancellationToken cancellationToken = default);

    Task RequestStop();
}
