namespace TechStack.Application.Common.Interfaces;

public interface IDataFetcherService
{
    public Task<Dictionary<string, string>> FetchDataAsync();
}
