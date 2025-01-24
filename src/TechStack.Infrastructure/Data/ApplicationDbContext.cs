namespace TechStack.Infrastructure.Data;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Domain.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
