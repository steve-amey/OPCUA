using DataCollectors.OPCUA.Core.Application.Examples;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Factories;

public interface IClientFactory
{
    IUAClient? Client { get; }

    Task CreateClient();
}