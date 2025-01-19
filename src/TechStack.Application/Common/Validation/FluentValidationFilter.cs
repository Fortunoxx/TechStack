namespace TechStack.Application.Common.Validation;

using FluentValidation;
using MassTransit;
using TechStack.Application.Common.Interfaces;

public class FluentValidationFilter<TMessage>(
    IValidator<TMessage> validator,
    IValidationFailurePipe<TMessage> failurePipe) : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    private readonly IValidationFailurePipe<TMessage> _failurePipe = failurePipe ?? throw new ArgumentNullException(nameof(failurePipe));
    private readonly IValidator<TMessage> _validator = validator;

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        if (_validator is null)
        {
            await next.Send(context);
            return;
        }

        var message = context.Message;
        var validationResult = await _validator.ValidateAsync(message, context.CancellationToken);

        if (validationResult.IsValid)
        {
            await next.Send(context);
            return;
        }

        var validationProblems = new FailureMessage(validationResult.Errors.ToErrorDictionary(), "Validation error");

        var failureContext = new ValidationFailureContext<TMessage>(context, validationProblems);
        await _failurePipe.Send(failureContext);
    }

    public void Probe(ProbeContext context) { }
}
