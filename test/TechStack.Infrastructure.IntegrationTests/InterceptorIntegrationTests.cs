namespace TechStack.Infrastructure.IntegrationTests;

using AutoFixture;
using FluentAssertions;
using Infrastructure.IntegrationTests.Fixtures;
using Infrastructure.IntegrationTests.Mocks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TechStack.Application.Common.Interfaces;
using TechStack.Domain.Entities;
using TechStack.Infrastructure;
using TechStack.Infrastructure.Data.Interceptors;

[Trait("Category", "IntegrationTest")]
public sealed class InterceptorIntegrationTests(DbContextFixture dbContextFixture) : IClassFixture<DbContextFixture>
{
    [Fact]
    internal async Task DbContextSaveChangesInterceptor_SaveChangesAsync_ShouldWorkAsync()
    {
        // Arrange
        var cut = CreateDefaultDbContext(dbContextFixture);
        var user = new Fixture().
            Build<User>().
            Without(x => x.Id).
            Without(x => x.CreatedBy).
            Without(x => x.LastModifiedBy).
            Create();

        // Act
        await cut.Users.AddAsync(user);
        var act = await cut.SaveChangesAsync();

        // Assert
        act.Should().Be(1);
    }

    [Fact]
    internal void DbContextSaveChangesInterceptor_SaveChanges_ShouldWork()
    {
        // Arrange
        var cut = CreateDefaultDbContext(dbContextFixture);
        var user = new Fixture().
            Build<User>().
            Without(x => x.Id).
            Without(x => x.CreatedBy).
            Without(x => x.LastModifiedBy).
            Create();

        // Act
        cut.Users.Add(user);
        var act = cut.SaveChanges();

        // Assert
        act.Should().Be(1);
    }

    private static ApplicationDbContext CreateDefaultDbContext(DbContextFixture dbContextFixture)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(dbContextFixture.SqlConnectionString)
        {
            InitialCatalog = DbContextFixture.InitialCatalog
        };

        var provider = new ServiceCollection().
            AddSingleton(_ => TimeProvider.System).
            AddScoped<IUser, MockUser>().
            AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>().
            // AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>().
            AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionStringBuilder.ConnectionString);
            }).BuildServiceProvider(true);
        var scope = provider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        return scopedServices.GetRequiredService<ApplicationDbContext>();
    }
}
