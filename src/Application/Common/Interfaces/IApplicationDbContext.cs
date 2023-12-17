namespace TechStack.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TechStack.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<TestItem> TestItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}