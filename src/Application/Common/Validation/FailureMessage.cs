using System.Net;

namespace TechStack.Application.Common.Validation;

public record FailureMessage(IDictionary<string, string[]> Errors, string Message);

public class ValidationException : Exception
{
    public ValidationException(IDictionary<string, string[]> errors, string detail, int statusCode = (int)HttpStatusCode.BadRequest) : base()
    {
        Detail = detail;
        Errors = errors;
        StatusCode = statusCode;
    }

    public string Detail { get; }

    public IDictionary<string, string[]> Errors { get; }

    public int StatusCode { get; }
}