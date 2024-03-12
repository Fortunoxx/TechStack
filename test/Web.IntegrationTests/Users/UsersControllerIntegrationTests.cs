namespace TechStack.Web.IntegrationTests.Users;

using FluentAssertions;
using TechStack.Infrastructure;
using Xunit;

public sealed class UsersControllerIntegrationTests : IClassFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
    private readonly IntegrationTestFactory<Program, ApplicationDbContext> _factory;

    public UsersControllerIntegrationTests(IntegrationTestFactory<Program, ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    internal async Task GetPersons_Returns_Valid_Result_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/users");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be some users");
        response.EnsureSuccessStatusCode();
    }
}