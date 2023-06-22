using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using MediatR;
using Opc.Ua;

namespace DataCollectors.OPCUA.Core.Application.Alarms.Commands;

public record AcknowledgeAlarmCommand : IRequest<Unit>
{
    public string NodeId { get; init; } = null!;

    public string EventId { get; init; } = null!;

    public string? Message { get; init; }
}

public class AcknowledgeAlarmCommandHandler : IRequestHandler<AcknowledgeAlarmCommand, Unit>
{
    private readonly IUaClientService _uaClientService;

    public AcknowledgeAlarmCommandHandler(IUaClientService uaClientService)
    {
        _uaClientService = uaClientService;
    }

    public Task<Unit> Handle(AcknowledgeAlarmCommand command, CancellationToken cancellationToken)
    {
        var conditionNodeId = NodeId.Parse(command.NodeId);

        var acknowledged = _uaClientService
            .AcknowledgeAlarm(conditionNodeId, command.EventId, command.Message);

        if (!acknowledged)
        {
            throw new Exception("Could not acknowledge the alarm");
        }

        return Task.FromResult(Unit.Value);
    }
}
