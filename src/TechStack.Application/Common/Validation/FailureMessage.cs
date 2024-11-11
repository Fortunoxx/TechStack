using System.Net;

namespace TechStack.Application.Common.Validation;

public record FailureMessage(IDictionary<string, string[]> Errors, string Message);

public class ValidationException(IDictionary<string, string[]> errors, string detail, int statusCode = (int)HttpStatusCode.BadRequest) : Exception()
{
    public string Detail { get; } = detail;

    public IDictionary<string, string[]> Errors { get; } = errors;

    public int StatusCode { get; } = statusCode;
}