namespace TechStack.Application.Common.Interfaces;

public interface ILockService
{
    bool CreateLock(int id, object data);
    bool DeleteLock(int id);
    IDictionary<int, object> GetAllLocks();
    KeyValuePair<int, object>? GetById(int id);
}
