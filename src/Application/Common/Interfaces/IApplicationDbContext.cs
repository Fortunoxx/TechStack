namespace TechStack.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TechStack.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
    // had to do this. used it in IntegrationTest c'tor
    int SaveChanges();
}
