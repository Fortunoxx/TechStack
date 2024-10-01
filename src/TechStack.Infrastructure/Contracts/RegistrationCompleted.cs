namespace TechStack.Infrastructure.Contracts;

using MassTransit.Courier.Contracts;

public record RegistrationCompleted : RoutingSlipCompleted
{
    public Guid SubmissionId { get; init; }
    
    public Guid TrackingNumber { get; init; }

    public DateTime Timestamp { get; set; }
    
    public TimeSpan Duration { get; set; }

    public required IDictionary<string, object> Variables { get; set; }
}
