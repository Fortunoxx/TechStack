namespace TechStack.Application.Common.Validation;

using MassTransit;
using MassTransit.Middleware;

public class ValidationFailureContext<TMessage>(ConsumeContext<TMessage> wrappedContext, FailureMessage validationProblems) :
    BasePipeContext(wrappedContext.CancellationToken), PipeContext
    where TMessage : class
{
    public ConsumeContext<TMessage> InnerContext { get; } = wrappedContext ?? throw new ArgumentNullException(nameof(wrappedContext));
    public FailureMessage ValidationProblems { get; } = validationProblems ?? throw new ArgumentNullException(nameof(validationProblems));
}