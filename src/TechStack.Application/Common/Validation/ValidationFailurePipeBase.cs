using MassTransit;
using TechStack.Application.Common.Interfaces;

namespace TechStack.Application.Common.Validation;

public abstract class ValidationFailurePipeBase<TMessage> : IValidationFailurePipe<TMessage>
    where TMessage : class
{
    public virtual void Probe(ProbeContext context)
        => context.CreateScope($"ValidationFailurePipe<{typeof(TMessage)}>");

    public abstract Task Send(ValidationFailureContext<TMessage> context);
}