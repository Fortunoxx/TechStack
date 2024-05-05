namespace TechStack.Web.Authentication;

public interface IApiKeyValidation
{
    bool IsValidApiKey(string userApiKey);
}
