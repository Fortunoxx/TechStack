namespace TechStack.Web.Infrastructure;

using TechStack.Application.Common.Validation;

public class ValidationFailurePipe<TMessage> : ValidationFailurePipeBase<TMessage>
    where TMessage : class
{
    public async override Task Send(ValidationFailureContext<TMessage> context)
        => throw new ValidationException(context.ValidationProblems.Errors);
}
