namespace TechStack.Application.Common.Models;

public record FaultedMessage(int HttpStatusCode, object Payload);