namespace TechStack.Application.Common.Interfaces;

public interface IAggregatorService
{
    public Task<IDictionary<string, string>> FetchAll();
}
