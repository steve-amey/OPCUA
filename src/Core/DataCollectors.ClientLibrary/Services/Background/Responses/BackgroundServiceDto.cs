using DataCollectors.ClientLibrary.Services.Background.Enums;

namespace DataCollectors.ClientLibrary.Services.Background.Responses;

public class BackgroundServiceDto
{
    public string Name { get; init; } = null!;

    public BackgroundServiceStatus Status { get; init; }
}
