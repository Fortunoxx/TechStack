namespace TechStack.Infrastructure;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Domain.Entities;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
