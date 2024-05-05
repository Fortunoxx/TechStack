namespace TechStack.Web.Authentication;

public class ApiKeyValidation(IConfiguration configuration) : IApiKeyValidation
{
    private readonly IConfiguration _configuration = configuration;

    public bool IsValidApiKey(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            return false;
        }

        string? apiKey = _configuration.GetValue<string>(AuthConstants.ApiKeySectionName);
        if (apiKey == null || apiKey != userApiKey)
        {
            return false;
        }

        return true;
    }
}