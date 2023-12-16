namespace TechStack.Infrastructure.Services;

using TechStack.Application.Common.Interfaces;

public class LockService : ILockService
{
    private IDictionary<int, object> _list = new Dictionary<int, object>();

    public bool CreateLock(int id)
    {
        _list.Add(id, Guid.NewGuid());
        return true;
    }

    public bool DeleteLock(int id)
    {
        if (_list.Any(x => x.Key == id))
        {
            _list.Remove(id);
            return true;
        }
        return false;
    }

    public IEnumerable<int> GetAllLocks() => _list.Select(x => x.Key);

    public int? GetById(int id)
    {
        if (_list.Any(x => x.Key == id))
            return id;
        return null;
    }
}