namespace TechStack.Web.IntegrationTests.Test;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoFixture;
using FluentAssertions;
using TechStack.Application.Common.Models;
using Xunit;

[Trait("Category", "IntegrationTest")]
public sealed class TestControllerIntegrationTests(IntegrationTestFactoryWithoutDb<Program> factory) :
    IClassFixture<IntegrationTestFactoryWithoutDb<Program>>
{
    private readonly IntegrationTestFactoryWithoutDb<Program> factory = factory;

    [Fact]
    internal async Task TestApi_GetSomeData_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();
        cut.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: TestAuthHandler.AuthenticationScheme);

        // Act
        var act = await cut.GetAsync("api/test/1");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be some entry");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task TestApi_CreateTestLock_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();
        cut.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: TestAuthHandler.AuthenticationScheme);
        var cmd = new Fixture().Create<UpsertLockCommand>();

        // Act
        var act = await cut.PostAsync("api/test/1", JsonContent.Create(cmd));

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be a new entry");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task TestApi_UpdateTestLock_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();
        cut.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: TestAuthHandler.AuthenticationScheme);
        var cmd = new Fixture().Create<UpsertLockCommand>();

        // Act
        var act = await cut.PutAsync("api/test/1", JsonContent.Create(cmd));

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "there should be an updated entry");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task TestApi_GetSomeAnonymousData_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = factory.CreateClient();

        // Act
        var act = await cut.GetAsync("api/test/anonymous/1");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be some data");
        act.EnsureSuccessStatusCode();
    }
}
