namespace TechStack.Application.Common.Models;

using System.Diagnostics.CodeAnalysis;
using System.Net;

[ExcludeFromCodeCoverage]
public record FaultedResponse(HttpStatusCode HttpStatusCode, object Payload);
