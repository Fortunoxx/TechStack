using TechStack.Infrastructure.Contracts;

namespace TechStack.Infrastructure.Services;

public interface ISecurePaymentInfoService
{
    SecurePaymentInfo GetPaymentInfo(string emailAddress, string cardNumber);
}

public class SecurePaymentInfoService :
    ISecurePaymentInfoService
{
    public SecurePaymentInfo GetPaymentInfo(string emailAddress, string cardNumber)
    {
        return new SecurePaymentInfo
        {
            CardNumber = cardNumber,
            VerificationCode = "666",
            CardholderName = "ROB GATES",
            ExpirationMonth = 12,
            ExpirationYear = 2025,
        };
    }
}
