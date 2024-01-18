namespace TechStack.Application.Common.Interfaces;

public interface ILockService
{
    bool CreateLock(int id, object data);
    bool DeleteLock(int id);
    IEnumerable<int> GetAllLocks();
    int? GetById(int id);
}
