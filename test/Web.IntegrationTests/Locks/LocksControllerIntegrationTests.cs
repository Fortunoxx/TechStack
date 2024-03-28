namespace TechStack.Web.IntegrationTests.Locks;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Common.Interfaces;
using Xunit;

[Trait("Category", "IntegrationTest")]
public sealed class LocksControllerIntegrationTests(IntegrationTestFactoryWithoutDb<Program> factory) : IClassFixture<IntegrationTestFactoryWithoutDb<Program>>
{
    private readonly IntegrationTestFactoryWithoutDb<Program> factory = factory;

    [Fact]
    internal async Task LocksApi_GetAll_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();

        // Act
        var act = await cut.GetAsync("api/locks");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task LocksApi_CreateLock_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();

        // Act
        var act = await cut.PostAsync("api/locks/1", null);

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task LocksApi_GetLockById_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();
        var scope = factory.Services.CreateAsyncScope();
        var lockService = scope.ServiceProvider.GetRequiredService<ILockService>();
        _ = lockService.CreateLock(2, Guid.NewGuid());

        // Act
        var act = await cut.GetAsync("api/locks/2");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task LocksApi_DeleteLock_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();
        var scope = factory.Services.CreateAsyncScope();
        var lockService = scope.ServiceProvider.GetRequiredService<ILockService>();
        _ = lockService.CreateLock(10, Guid.NewGuid());

        // Act
        var act = await cut.DeleteAsync("api/locks/10");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        act.EnsureSuccessStatusCode();
    }
}
