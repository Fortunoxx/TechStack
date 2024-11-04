namespace TechStack.Infrastructure.Components.Activities;

public record ProcessPaymentArguments
{
    public required string CardNumber { get; init; }
 
    public required string VerificationCode { get; init; }
 
    public required string CardholderName { get; init; }
 
    public int ExpirationMonth { get; init; }
 
    public int ExpirationYear { get; init; }

    public decimal Amount { get; init; }
}