namespace TechStack.Application.Common.Interfaces;

public interface ILockService
{
    Task<bool> CreateLock(int id, object data);

    Task<bool> DeleteLock(int id);
    
    Task<IEnumerable<int>?> GetAllLocks();
    
    Task<int?> GetById(int id);
}
