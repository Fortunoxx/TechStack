namespace TechStack.Infrastructure.Data;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Domain.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostType> PostTypes => Set<PostType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<VoteType> VoteTypes => Set<VoteType>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
