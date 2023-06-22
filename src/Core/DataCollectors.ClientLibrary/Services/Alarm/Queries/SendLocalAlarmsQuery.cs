using DataCollectors.ClientLibrary.Contracts.Enums;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Services.Response;
using MediatR;

namespace DataCollectors.ClientLibrary.Services.Alarm.Queries;

public record SendLocalAlarmsQuery : IRequest<EmptyServiceResponse>
{
    public WebApiNames System { get; init; }
}

public class SendLocalAlarmsQueryHandler : IRequestHandler<SendLocalAlarmsQuery, EmptyServiceResponse>
{
    private readonly IAlarmService _alarmService;
    private readonly IServiceResponseGenerator _serviceResponseGenerator;

    public SendLocalAlarmsQueryHandler(IAlarmService alarmService, IServiceResponseGenerator serviceResponseGenerator)
    {
        _alarmService = alarmService;
        _serviceResponseGenerator = serviceResponseGenerator;
    }

    public Task<EmptyServiceResponse> Handle(SendLocalAlarmsQuery request, CancellationToken cancellationToken)
    {
        _ = _alarmService
            .SendStoredItems(request.System);

        var result = _serviceResponseGenerator
            .CreateEmptyServiceResponse();

        return Task.FromResult(result);
    }
}
