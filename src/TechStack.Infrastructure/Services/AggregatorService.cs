namespace TechStack.Infrastructure.Services;

using TechStack.Application.Common.Interfaces;

public class AggregatorService(IEnumerable<IDataFetcherService> fetchers) : IAggregatorService
{
    private readonly IEnumerable<IDataFetcherService> fetchers = fetchers;

    public async Task<IDictionary<string, string>> FetchAll()
    {
        var result = new SortedDictionary<string, string>();

        // fill result with data from fetchers
        foreach(var fetcher in fetchers)
        {
            var data = await fetcher.FetchDataAsync();
            foreach (var item in data.Where(x => NameValueKeys.Keys.Contains(x.Key)))
            {
                result[item.Key] = item.Value;
            }
        }

        // add missing keys with default values
        foreach (var key in NameValueKeys.Keys)
        {
            if (!result.ContainsKey(key))
            {
                result[key] = string.Empty;
            }
        }

        return result;
    }
}
