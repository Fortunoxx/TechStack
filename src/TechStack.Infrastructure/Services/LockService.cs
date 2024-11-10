namespace TechStack.Infrastructure.Services;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TechStack.Application.Common.Interfaces;

public class LockService(IDistributedCache distributedCache) : ILockService
{
    private static string CacheKey => $"{nameof(LockService)}";

    public async Task<bool> CreateLock(int id, object data)
    {
        var dataDictionary = await GetDataDictionary();
        dataDictionary.Add(id, data ?? Guid.NewGuid());
        await distributedCache.SetAsync(CacheKey, ConvertDictionaryToByteArray(dataDictionary));
        return true;
    }

    public async Task<bool> DeleteLock(int id)
    {
        var dataDictionary = await GetDataDictionary();

        if (dataDictionary.Any(x => x.Key == id))
        {
            dataDictionary.Remove(id);
            await distributedCache.SetAsync(CacheKey, ConvertDictionaryToByteArray(dataDictionary));
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<int>?> GetAllLocks()
        => (await GetDataDictionary())?.Select(x => x.Key);

    public async Task<int?> GetById(int id)
    {
        var dataDictionary = ConvertByteArrayToDictionary(await distributedCache.GetAsync(CacheKey));

        if (dataDictionary.Any(x => x.Key == id))
        {
            return id;
        }

        return null;
    }

    private static byte[] ConvertDictionaryToByteArray(Dictionary<int, object> dictionary)
    {
        var jsonString = JsonSerializer.Serialize(dictionary);
        return System.Text.Encoding.UTF8.GetBytes(jsonString);
    }

    private static Dictionary<int, object> ConvertByteArrayToDictionary(byte[] byteArray)
    {
        var jsonString = System.Text.Encoding.UTF8.GetString(byteArray);
        return JsonSerializer.Deserialize<Dictionary<int, object>>(jsonString);
    }

    private async Task<Dictionary<int, object>> GetDataDictionary()
    {
        var byteArray = await distributedCache.GetAsync(CacheKey);
        return byteArray != null ? ConvertByteArrayToDictionary(byteArray) : [];
    }
}
