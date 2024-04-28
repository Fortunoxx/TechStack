namespace TechStack.Application.Common.Models;

using System.Net;

public record FaultedResponse(HttpStatusCode HttpStatusCode, object Payload);
