using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Domain.Entities;

namespace TechStack.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    public DbSet<TestItem> TestItems => Set<TestItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}
