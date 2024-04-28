namespace TechStack.Application.Common.Interfaces;

using MassTransit;
using TechStack.Application.Common.Validation;

public interface IValidationFailurePipe<TMessage> :
    IPipe<ValidationFailureContext<TMessage>>
    where TMessage : class
{ }
