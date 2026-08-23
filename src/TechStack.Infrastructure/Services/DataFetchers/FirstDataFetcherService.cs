namespace TechStack.Infrastructure.Services.DataFetchers;

using TechStack.Application.Common.Interfaces;

public class FirstDataFetcherService : IDataFetcherService
{
    public Task<Dictionary<string, string>> FetchDataAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            { "First", "First" },
        });
    }
}
