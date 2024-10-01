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

    public IEnumerable<int> GetAllLocks() => dataDictionary.Select(x => x.Key);

    public int? GetById(int id)
    {
        if (dataDictionary.Any(x => x.Key == id))
            return id;
        return null;
    }
}
