namespace TechStack.Web.IntegrationTests;

using System.Net.Mime;
using Microsoft.Extensions.DependencyInjection;
using AutoBogus;
using AutoBogus.Conventions;
using AutoFixture;
using FluentAssertions;
using TechStack.Application.Users.Commands;
using TechStack.Domain.Entities;
using TechStack.Infrastructure;
using TechStack.Web.IntegrationTests.Mocks;
using TechStack.Web.IntegrationTests.Fixtures;
using Xunit;

[Trait("Category", "IntegrationTest")]
public sealed class UsersControllerIntegrationTests : IAsyncLifetime,
    IClassFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
    private readonly IntegrationTestFactory<Program, ApplicationDbContext> _factory;

    private readonly ApplicationDbContext _context;

    public UsersControllerIntegrationTests(IntegrationTestFactory<Program, ApplicationDbContext> factory)
    {
        _factory = factory;
        var scope = factory.Services.CreateAsyncScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task InitializeAsync() => await SeedDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    internal async Task UsersApi_GetPersons_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();

        // Act
        var act = await cut.GetAsync("api/users");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be some users");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task UsersApi_GetPersonById_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();

        // Act
        var act = await cut.GetAsync("api/users/1");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be a user with id 1");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task UsersApi_CreatePerson_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();

        var user = new Fixture().Build<AddUserCommand>()
            .With(x => x.DisplayName, $"DisplayName-{Guid.NewGuid()}"[..40])
            .With(x => x.EmailHash, $"EmailHash-{Guid.NewGuid()}"[..40])
            .Create();

        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(user);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        var act = await cut.PostAsync("api/users", body);

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.Created, "there should be a new user");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task UsersApi_DeletePersonById_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();

        // Act
        var act = await cut.DeleteAsync("api/users/2");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "user should be deleted");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task UsersApi_UpdatePersonById_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();

        var user = new Fixture().Build<AlterUserCommandPart>()
            .With(x => x.DisplayName, $"DisplayName-{Guid.NewGuid()}"[..40])
            .With(x => x.EmailHash, $"EmailHash-{Guid.NewGuid()}"[..40])
            .Create();

        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(user);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        var act = await cut.PutAsync("api/users/1", body);

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "user should be updated");
        act.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task UsersApi_UpdatePersonByInvalidId_ShouldReturnFaultedResponseAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();

        var user = new Fixture().Build<AlterUserCommandPart>()
            .With(x => x.DisplayName, $"DisplayName-{Guid.NewGuid()}"[..40])
            .With(x => x.EmailHash, $"EmailHash-{Guid.NewGuid()}"[..40])
            .Create();

        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(user);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        var act = await cut.PutAsync("api/users/-1", body);

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound, "no user with this id should exist");
    }

    private async Task SeedDatabaseAsync()
    {
        // Create a new instance of the Faker class
        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("de");
            builder.WithConventions(cfg =>
            {
                cfg.StreetName.Aliases("Street", "Strasse", "Straße");
                cfg.PhoneNumber.Aliases("Phone", "Mobile", "Tel", "Telefon", "Fax", "Mobil", "Rufnummer");
                cfg.ZipCode.Aliases("PostalCode", "PLZ", "Postleitzahl");
            });
            builder.WithSkip<User>(x => x.Id);
        });

        // Generate fake data for a list of customers
        var userFaker = new UserFaker(Constants.EmailProvider);
        var users = userFaker.Generate(100);

        // Add the customers to the context and save changes
        _context.Users.AddRange(users);
        _ = await _context.SaveChangesAsync(new CancellationTokenSource().Token);
    }
}