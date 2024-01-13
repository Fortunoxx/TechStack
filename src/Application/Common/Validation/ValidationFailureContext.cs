namespace TechStack.Application.Common.Validation;

using MassTransit;
using MassTransit.Middleware;

public class ValidationFailureContext<TMessage> :
    BasePipeContext, PipeContext
    where TMessage : class
{
    public ValidationFailureContext(ConsumeContext<TMessage> wrappedContext, IDictionary<string, string[]> validationProblems) :
        base(wrappedContext.CancellationToken)
    {
        InnerContext = wrappedContext ?? throw new ArgumentNullException(nameof(wrappedContext));
        ValidationProblems = validationProblems ?? throw new ArgumentNullException(nameof(validationProblems));
    }

    public ConsumeContext<TMessage> InnerContext { get; }
    public IDictionary<string, string[]> ValidationProblems { get; }
}