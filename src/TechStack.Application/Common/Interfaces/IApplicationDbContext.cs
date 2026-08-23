namespace TechStack.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TechStack.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<Badge> Badges { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Post> Posts { get; }
    DbSet<PostType> PostTypes { get; }
    DbSet<User> Users { get; }
    DbSet<Vote> Votes { get; }
    DbSet<VoteType> VoteTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
