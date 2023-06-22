using DataCollectors.ClientLibrary.Validators;
using FluentValidation;

namespace DataCollectors.ClientLibrary.Services.Alarm.Queries;

public class SendLocalAlarmsQueryValidator : BaseValidator<SendLocalAlarmsQuery>
{
    public SendLocalAlarmsQueryValidator()
    {
        RuleFor(x => x.System)
            .IsInEnum();
    }
}