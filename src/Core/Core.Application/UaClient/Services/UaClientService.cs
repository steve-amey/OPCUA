using DataCollectors.OPCUA.Core.Application.UaClient.Factories;
using DataCollectors.OPCUA.Core.Application.UaClient.Helpers;
using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Services;

internal class UaClientService : IUaClientService
{
    private readonly ILogger _logger;
    private uint _subscriptionId;

    public UaClientService(IClientFactory clientFactory, ILogger<UaClientService> logger)
    {
        ClientFactory = clientFactory;
        _logger = logger;
    }

    public IClientFactory ClientFactory { get; }

    public bool ClientCreated => ClientFactory.Client != null;

    public bool IsConnected => ClientFactory.Client != null && Client.Session != null && Session.Connected;

    public IList<string> Namespaces => Client.Session != null ? Session.NamespaceUris.ToArray() : new List<string>();

    private UAClient Client => (UAClient)ClientFactory.Client! ?? throw new Exception("Client not created");

    private Session Session => (Session)Client.Session;

    public void CreateClient()
    {
        ClientFactory.CreateClient();
    }

    public async Task<bool> Connect(string serverUrl, bool transferSubscriptionsOnReconnect = true, bool useSecurity = true)
    {
        var connected = await Client.ConnectAsync(serverUrl, useSecurity);
        if (connected && transferSubscriptionsOnReconnect)
        {
            Client.Session.TransferSubscriptionsOnReconnect = true;
        }

        return connected;
    }

    public Subscription CreateSubscription(string name, uint minLifeTime)
    {
        var subscription = new Subscription(Session.DefaultSubscription)
        {
            DisplayName = name,
            PublishingEnabled = true,
            PublishingInterval = 1000,
            LifetimeCount = 0,
            MinLifetimeInterval = minLifeTime
        };

        Session.AddSubscription(subscription);

        // Create the subscription on Server side
        subscription.Create();

        _subscriptionId = subscription.Id;

        _logger.LogInformation("New Subscription created with SubscriptionId = {0}.", subscription.Id);

        return subscription;
    }

    public MonitoredItem CreateMonitoredItem(Subscription subscription, NodeId nodeId)
    {
        var filter = new EventFilter();

        // These are the defaults for a new event filter anyway, just added them here for visibility....
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.EventId);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.EventType);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.SourceNode);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.SourceName);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.Time);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.ReceiveTime);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.LocalTime);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.Message);
        filter.AddSelectClause(ObjectTypes.BaseEventType, BrowseNames.Severity);

        // Add Condition-related attributes to the filter so that we have enough info to determine
        // Enabled/Disabled, Active/Inactive including time of active transition, Acked/Unacked and active subcondition
        filter.AddSelectClause(ObjectTypes.AcknowledgeableConditionType, BrowseNames.AckedState);
        filter.AddSelectClause(ObjectTypes.AcknowledgeableConditionType, null, Attributes.NodeId); // Requests condition id

        filter.AddSelectClause(ObjectTypes.AlarmConditionType, string.Join("/", BrowseNames.ActiveState, BrowseNames.Id), Attributes.Value);
        filter.AddSelectClause(ObjectTypes.AlarmConditionType, BrowseNames.ActiveState, Attributes.Value);
        filter.AddSelectClause(ObjectTypes.AlarmConditionType, string.Join("/", BrowseNames.ActiveState, BrowseNames.TransitionTime), Attributes.Value);
        filter.AddSelectClause(ObjectTypes.AlarmConditionType, string.Join("/", BrowseNames.ActiveState, BrowseNames.EffectiveDisplayName), Attributes.Value);

        var displayName = Session.NodeCache.GetDisplayText((ExpandedNodeId)nodeId);

        var monitoredItem = new MonitoredItem(subscription.DefaultItem)
        {
            StartNodeId = nodeId,
            AttributeId = Attributes.EventNotifier,
            MonitoringMode = MonitoringMode.Reporting,
            DisplayName = displayName,
            DiscardOldest = true,
            QueueSize = 0,
            SamplingInterval = 0,
            NodeClass = NodeClass.Object,
            Filter = filter
        };

        subscription.AddItem(monitoredItem);

        return monitoredItem;
    }

    public ServerStatusDataType GetServerStatus()
    {
        // Get the current DataValue object for the ServerStatus node
        var nodeId = new NodeId(Variables.Server_ServerStatus);
        var dataValue = Session.ReadValue(nodeId);

        // Unpack the ExtensionObject that the DataValue contains, then return ServerStatusDataType object
        // that represents the current server status
        var extensionObject = (ExtensionObject)dataValue.Value;
        var serverStatus = (ServerStatusDataType)extensionObject.Body;

        return serverStatus;
    }

    public INode GetEventType(NodeId eventType)
    {
        var typeNode = Session.NodeCache.Find(eventType);
        return typeNode;
    }

    public DataValue? GetNodeValue(string nodeName)
    {
        var nodeId = uint.TryParse(nodeName, out var i) ? new NodeId(i) : NodeId.Parse(nodeName);

        var result = GetNodeValue(nodeId);
        return result;
    }

    public DataValue? GetNodeValue(NodeId nodeId)
    {
        try
        {
            var dataValue = Session.ReadValue(nodeId);

            if (dataValue.StatusCode == StatusCodes.Good)
            {
                return dataValue;
            }

            _logger.LogError("GetNodeValue for nodeId '{nodeId}' return status code: {statusCode}", nodeId, dataValue.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, ex.Message);
        }

        return null;
    }

    public IList<NodeValueDto> GetNodeValues(IList<NodeId> nodeIds)
    {
        var result = new List<NodeValueDto>();

        try
        {
            Session.ReadValues(nodeIds, out var values, out var errors);

            for (int i = 0; i < nodeIds.Count; i++)
            {
                var dataValue = values[i];

                var nodeValue = new NodeValueDto
                {
                    NodeId = nodeIds[i].ToString(),
                    ServerTimestamp = dataValue.ServerTimestamp,
                    SourceTimestamp = dataValue.SourceTimestamp,
                    StatusCode = dataValue.StatusCode.ToString(),
                    Value = dataValue.Value.ToString()
                };

                result.Add(nodeValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, ex.Message);
        }

        return result;
    }

    public ReferenceDescriptionCollection BrowseNode(NodeId nodeId)
    {
        var browser = new Browser(Session)
        {
            BrowseDirection = BrowseDirection.Forward,
            NodeClassMask = (int)NodeClass.Object | (int)NodeClass.Variable,
            ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences
        };

        //Session
        //    .Browse(null, null, nodeId, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
        //        out byte[] bytes,
        //        out ReferenceDescriptionCollection references);

        //foreach (var reference in references)
        //{
        //    Console.WriteLine("DisplayName: " + reference.DisplayName + " BrowseName: " + reference.BrowseName + " NodeClass: " + reference.NodeClass);

        //    ReferenceDescriptionCollection NewRDC;
        //    byte[] NewArrBytes;
        //    Session.Browse(null, null, ExpandedNodeId.ToNodeId(reference.NodeId, Session.NamespaceUris), 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out NewArrBytes, out NewRDC);
        //    foreach (var newRD in NewRDC)
        //    {
        //        Console.WriteLine("NEW - DisplayName: " + newRD.DisplayName + " BrowseName: " + newRD.BrowseName + " NodeClass: " + newRD.NodeClass);
        //    }
        //}

        var browseResults = browser.Browse(nodeId);

        return browseResults;
    }

    public IList<InstanceDeclaration> CreateInstanceDeclarationsForType(NodeId nodeId)
    {
        var result = ClientUtils.CollectInstanceDeclarationsForType(Session, nodeId);
        return result;
    }

    public void RefreshAlarms()
    {
        try
        {
            var o = Session
                .Call(ObjectTypeIds.ConditionType, MethodIds.ConditionType_ConditionRefresh, new List<Variant> { new(_subscriptionId) });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool AcknowledgeAlarm(NodeId nodeId, string eventId, string? message)
    {
        var originalBytes = Convert.FromHexString(eventId);

        var callParams = new List<Variant>
        {
            new (originalBytes),
            new (new LocalizedText(message))
        };

        var result = false;

        try
        {
            var callResult = Session
                .Call(nodeId, MethodIds.AcknowledgeableConditionType_Acknowledge, callParams);

            result = callResult.Count == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alarm");
        }

        return result;
    }

    public bool AcknowledgeAlarms(NodeId nodeId, string message)
    {
        var methods = new CallMethodRequestCollection();

        //foreach (var alarm in m_alarmList)
        //{
        //    methods.Add(new CallMethodRequest
        //    {
        //        ObjectId = alarm.EventFields[0].ToNodeId(),
        //        MethodId = MethodIds.AcknowledgeableConditionType_Acknowledge,
        //        InputArguments = new VariantCollection()
        //        {
        //            new Variant(alarm.EventFields[1].ToByteString()),
        //            new Variant(new LocalizedText(message))
        //        }
        //    });
        //}

        var responseHeader = Session.Call(null, methods, out var results, out var diagnosticInfos);

        return true;
    }
}
