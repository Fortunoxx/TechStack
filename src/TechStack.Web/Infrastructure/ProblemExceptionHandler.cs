namespace TechStack.Web.Infrastructure;

using System;
using System.Collections;
using System.Threading;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Validation;
using TechStack.Domain.Common;

public class ProblemExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ProblemException problemException)
        {
            var problemDetails = new ProblemDetails
            {
                Title = problemException.Error,
                Status = StatusCodes.Status500InternalServerError,
                Detail = problemException.Message,
                Type = "Internal Server Error",
                Extensions = problemException.Data
                    .Cast<DictionaryEntry>()
                    .ToDictionary(x => x.Key.ToString()!, x => x.Value),
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }

        if (exception is MassTransit.RequestException rex && rex.InnerException is ValidationException vex)
        {
            var httpValidationProblemDetails = new HttpValidationProblemDetails
            {
                Title = "Validation Error",
                Type = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = vex.Detail,
                Errors = vex.Errors,
            };

            httpContext.Response.StatusCode = vex.StatusCode;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = httpValidationProblemDetails
            });
        }

        return true;
    }
}
