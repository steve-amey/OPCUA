using FluentValidation;

namespace DataCollectors.ClientLibrary.Validators;

public class BaseValidator<T> : AbstractValidator<T>
    where T : class
{
}