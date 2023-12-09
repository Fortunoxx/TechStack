namespace TechStack.Services;

public interface ILockService
{
    bool ReleaseLock(int id);
    bool AquireLock (int id);
    IEnumerable<int> GetAllLocks();
}

public class LockService : ILockService
{
    private IDictionary<int, object> _list = new Dictionary<int, object>();

    public bool AquireLock(int id)
    {
        _list.Add(id, Guid.NewGuid());
        return true;
    }

    public IEnumerable<int> GetAllLocks() => _list.Select(x => x.Key);

    public bool ReleaseLock(int id)
    {
        if (_list.Any(x => x.Key == id))
        {
            _list.Remove(id);
            return true;
        }
        return false;
    }
}