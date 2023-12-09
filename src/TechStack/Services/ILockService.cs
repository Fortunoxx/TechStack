namespace TechStack.Services;

public interface ILockService
{
    bool DeleteLock(int id);
    bool CreateLock (int id);
    IEnumerable<int> GetAllLocks();
}

public class LockService : ILockService
{
    private IDictionary<int, object> _list = new Dictionary<int, object>();

    public bool CreateLock(int id)
    {
        _list.Add(id, Guid.NewGuid());
        return true;
    }

    public IEnumerable<int> GetAllLocks() => _list.Select(x => x.Key);

    public bool DeleteLock(int id)
    {
        if (_list.Any(x => x.Key == id))
        {
            _list.Remove(id);
            return true;
        }
        return false;
    }
}