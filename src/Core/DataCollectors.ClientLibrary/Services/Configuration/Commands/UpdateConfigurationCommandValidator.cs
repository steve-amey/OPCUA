using DataCollectors.ClientLibrary.Validators;
using FluentValidation;

namespace DataCollectors.ClientLibrary.Services.Configuration.Commands;

public class UpdateConfigurationCommandValidator : BaseValidator<UpdateConfigurationCommand>
{
    public UpdateConfigurationCommandValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty();
    }
}