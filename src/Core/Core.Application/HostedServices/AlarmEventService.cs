using System.Diagnostics;
using DataCollectors.OPCUA.Core.Application.Shared.Options;
using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Client;

namespace DataCollectors.OPCUA.Core.Application.HostedServices;

public class AlarmEventService : BackgroundService
{
    private readonly OPCUASettings _opcuaSettings;
    private readonly ILogger<AlarmEventService> _logger;
    private readonly IUaClientService _uaClientService;
    private bool _isSubscribed;

    public AlarmEventService(
        ILogger<AlarmEventService> logger,
        IOptions<OPCUASettings> opcuaSettings,
        IUaClientService uaClientService)
    {
        _logger = logger;
        _opcuaSettings = opcuaSettings.Value;
        _uaClientService = uaClientService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started at: {time}", DateTimeOffset.Now.ToString("G"));

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();

        _logger.LogInformation("Stopped at: {time}", DateTimeOffset.Now.ToString("G"));

        await base.StopAsync(cancellationToken);

        _logger.LogInformation("Service took {ms} ms to stop.", stopWatch.ElapsedMilliseconds);
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken);
    }

    public Task RequestStop()
    {
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_uaClientService.ClientCreated)
            {
                _uaClientService.CreateClient();
            }

            if (!_uaClientService.IsConnected)
            {
                var serverUrl = new Uri(_opcuaSettings.ServerUrl);

                var connected = await _uaClientService.Connect(serverUrl.ToString(), useSecurity: false);
                if (connected && !_isSubscribed)
                {
                    CreateSubscriptions();
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private void CreateSubscriptions()
    {
        try
        {
            // Create a subscription for receiving data change/event notifications
            foreach (var appSettingSubscription in _opcuaSettings.Subscriptions)
            {
                var subscription = _uaClientService
                    .CreateSubscription(appSettingSubscription.Name);

                var eventMonitoredItem = _uaClientService
                    .CreateMonitoredItem(subscription, ObjectIds.Server);

                eventMonitoredItem.Notification += OnMonitoredItemNotification;

                // Create the monitored items on Server side
                subscription.ApplyChanges();

                _logger.LogInformation("MonitoredItems created for SubscriptionId = {id}.", subscription.Id);
            }

            _isSubscribed = true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Subscribe error: {error}", ex.Message);
        }
    }

    private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
    {
        try
        {
            if (e.NotificationValue is EventFieldList eventFields)
            {
                _ = PublishAlarm(monitoredItem, eventFields);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("OnMonitoredItemNotification error: {0}", ex.Message);
        }
    }

    private async Task PublishAlarm(MonitoredItem monitoredItem, EventFieldList eventFields)
    {
        var eventValues = new Dictionary<string, object?>();

        var eventFilter = (EventFilter)monitoredItem.Filter;
        foreach (var selectClause in eventFilter.SelectClauses)
        {
            var fieldName = selectClause.BrowsePath.Any()
                ? SimpleAttributeOperand.Format(selectClause.BrowsePath).Substring(1)
                : string.Empty;

            var fieldValue = monitoredItem.GetFieldValue(eventFields, selectClause.TypeDefinitionId, fieldName, selectClause.AttributeId);

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                fieldName = "ConditionId";
            }

            eventValues[fieldName] = fieldValue;
        }

        var eventId = (byte[])eventValues[BrowseNames.EventId]!;
        var eventType = (NodeId)eventValues[BrowseNames.EventType]!;
        var sourceNode = (NodeId)eventValues[BrowseNames.SourceNode]!;
        var conditionNode = (NodeId)eventValues["ConditionId"]!;

        var eventIdString = Convert.ToHexString(eventId);

        eventValues["EventIdHex"] = eventIdString;

        var typeNode = _uaClientService
            .GetEventType(eventType);

        var eventTypeDescription = typeNode.ToString();
        eventValues["EventTypeDescription"] = eventTypeDescription;

        var currentValue = _uaClientService
            .GetNodeValue(sourceNode);

        eventValues["CurrentValue"] = currentValue;

        var instanceDeclarations = _uaClientService
            .CreateInstanceDeclarationsForType(sourceNode);

        var readNodeIdList = new List<NodeId>();

        foreach (string field in _opcuaSettings.Fields)
        {
            var readNode = instanceDeclarations.SingleOrDefault(x => x.NodeId.ToString().EndsWith(field));

            if (readNode != null)
            {
                readNodeIdList.Add(readNode.NodeId);
            }
        }

        var nodeValues = _uaClientService
            .GetNodeValues(readNodeIdList);

        foreach (var nodeValueDto in nodeValues)
        {
            var key = nodeValueDto.NodeId.Split(".").Last();
            eventValues[key] = nodeValueDto.Value;
        }

        // TODO: Try acknowledging the alarm here to begin with, then get the api method working with the correct parameters
        _uaClientService
            .AcknowledgeAlarm(conditionNode, eventIdString, "Auto Acknowledged");
    }

    private object? ConvertFieldValue(object fieldValue)
    {
        if (fieldValue is byte[])
        {
            return fieldValue;
        }

        if (fieldValue is NodeId)
        {
            return fieldValue.ToString();
        }

        if (fieldValue is DateTime)
        {
            return fieldValue;
        }

        if (fieldValue is ushort)
        {
            return fieldValue;
        }

        if (fieldValue is ExtensionObject eo)
        {
            if (ExtensionObject.IsNull(eo))
            {
                return null;
            }

            return fieldValue;
        }

        if (fieldValue is LocalizedText lt)
        {
            return lt.Text;
        }

        return fieldValue;
    }
}