namespace TechStack.Infrastructure.Services;

using TechStack.Application.Common.Interfaces;

public class LockService : ILockService
{
    private readonly Dictionary<int, object> dataDictionary = [];

    public bool CreateLock(int id, object data)
    {
        dataDictionary.Add(id, data ?? Guid.NewGuid());
        return true;
    }

    public bool DeleteLock(int id)
    {
        if (dataDictionary.Any(x => x.Key == id))
        {
            dataDictionary.Remove(id);
            return true;
        }
        return false;
    }

    public IDictionary<int, object> GetAllLocks()
        => dataDictionary;

    public KeyValuePair<int, object>? GetById(int id)
        => dataDictionary.Any(x => x.Key == id) 
            ? dataDictionary.Single(x => x.Key == id) 
            : null;
}