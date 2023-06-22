using DataCollectors.OPCUA.Core.Application.UaClient.Helpers;
using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using Opc.Ua;
using Opc.Ua.Client;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Services;

public interface IUaClientService
{
    public bool ClientCreated { get; }

    public bool IsConnected { get; }

    public IList<string> Namespaces { get; }

    public void CreateClient();

    public Task<bool> Connect(string serverUrl, bool transferSubscriptionsOnReconnect = true, bool useSecurity = true);

    Subscription CreateSubscription(string name, uint minLifeTime = 120_000);

    MonitoredItem CreateMonitoredItem(Subscription subscription, NodeId nodeId);

    ServerStatusDataType GetServerStatus();

    INode GetEventType(NodeId eventType);

    DataValue? GetNodeValue(string nodeId);

    DataValue? GetNodeValue(NodeId nodeId);

    IList<NodeValueDto> GetNodeValues(IList<NodeId> nodeIds);

    ReferenceDescriptionCollection BrowseNode(NodeId nodeId);

    IList<InstanceDeclaration> CreateInstanceDeclarationsForType(NodeId nodeId);

    void RefreshAlarms();

    bool AcknowledgeAlarm(NodeId nodeId, string eventId, string? message);
}
