namespace TechStack.Application.Common.Validation;

using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;

public class FluentValidationFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    private readonly IValidationFailurePipe<TMessage> _failurePipe;
    private readonly ILogger<FluentValidationFilter<TMessage>> _logger;
    private readonly IValidator<TMessage> _validator;

    public FluentValidationFilter(ILogger<FluentValidationFilter<TMessage>> logger, IValidator<TMessage> validator, IValidationFailurePipe<TMessage> failurePipe)
    {
        _failurePipe = failurePipe ?? throw new ArgumentNullException(nameof(failurePipe));
        _logger = logger;
        _validator = validator;
    }

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

        var validationProblems = validationResult.Errors.ToErrorDictionary();

        var failureContext = new ValidationFailureContext<TMessage>(context, validationProblems);
        await _failurePipe.Send(failureContext);
    }

    public void Probe(ProbeContext context) { }
}
