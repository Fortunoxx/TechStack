namespace TechStack.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechStack.Domain.Entities;

public class TestItemConfiguration : IEntityTypeConfiguration<TestItem>
{
    public void Configure(EntityTypeBuilder<TestItem> builder)
    {
        builder.Property(t => t.Text).HasMaxLength(200).IsRequired();
    }
}
