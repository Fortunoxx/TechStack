namespace TechStack.Application.Common.Validation;

using FluentValidation.Results;
using MassTransit;

public static class ValidationFailureExtensions
{
    public static IDictionary<string, string[]> ToErrorDictionary(this IEnumerable<ValidationFailure> validationProblems)
    {
        return validationProblems
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.ErrorMessage).ToArray());
    }
}