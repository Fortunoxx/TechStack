namespace TechStack.Application.Common.Validation;

using MassTransit;
using MassTransit.Middleware;

public class ValidationFailureContext<TMessage> :
    BasePipeContext, PipeContext
    where TMessage : class
{
    public ValidationFailureContext(ConsumeContext<TMessage> wrappedContext, FailureMessage validationProblems) :
        base(wrappedContext.CancellationToken)
    {
        InnerContext = wrappedContext ?? throw new ArgumentNullException(nameof(wrappedContext));
        ValidationProblems = validationProblems ?? throw new ArgumentNullException(nameof(validationProblems));
    }

    public ConsumeContext<TMessage> InnerContext { get; }
    public FailureMessage ValidationProblems { get; }
}