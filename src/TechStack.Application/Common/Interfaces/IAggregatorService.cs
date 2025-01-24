namespace TechStack.Application.Common.Interfaces;

public interface IAggregatorService
{
    public Task<IDictionary<string, string>> FetchAll();
}

public class AggregatorService(IEnumerable<IDataFetcherService> fetchers) : IAggregatorService
{
    private IEnumerable<IDataFetcherService> fetchers = fetchers;

    public async Task<IDictionary<string, string>> FetchAll()
    {
        IDictionary<string, string> result = new Dictionary<string, string>();

        foreach(var fetcher in fetchers)
        {
            var data = await fetcher.FetchDataAsync();
            foreach(var item in data)
            {
                result[item.Key] = item.Value;
            }
        }

        return result;
    }
}

public interface IDataFetcherService
{
    public Task<Dictionary<string, string>> FetchDataAsync();
}

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

public class DefaultDataFetcherService : IDataFetcherService
{
    public Task<Dictionary<string, string>> FetchDataAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            { "Default", "Default" },
        });
    }
}