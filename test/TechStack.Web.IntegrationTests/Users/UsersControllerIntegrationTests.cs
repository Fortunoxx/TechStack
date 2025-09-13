namespace TechStack.Web.IntegrationTests.Users;

using System.Net.Mime;
using Microsoft.Extensions.DependencyInjection;
using AutoBogus;
using AutoBogus.Conventions;
using AutoFixture;
using AwesomeAssertions;
using TechStack.Application.Users.Commands;
using TechStack.Domain.Entities;
using TechStack.Web.IntegrationTests.Mocks;
using TechStack.Web.IntegrationTests.Fixtures;
using Xunit;
using TechStack.Infrastructure.Data;

[Trait("Category", "Integration")]
public sealed class UsersControllerIntegrationTests : IAsyncLifetime,
    IClassFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
    private readonly IntegrationTestFactory<Program, ApplicationDbContext> _factory;
    private readonly ApplicationDbContext _context;
    private readonly Func<Task> _resetDatabaseAsync;

    public UsersControllerIntegrationTests(IntegrationTestFactory<Program, ApplicationDbContext> factory)
    {
        _factory = factory;
        _resetDatabaseAsync = factory.ResetDatabaseAsync;
        var scope = factory.Services.CreateAsyncScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task InitializeAsync() => await SeedDatabaseAsync();

    public Task DisposeAsync() => _resetDatabaseAsync();

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
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "user should be deleted");
        act.EnsureSuccessStatusCode();
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

            // exclude all the auto-generated Ids
            builder.WithSkip<User>(x => x.Id);
            builder.WithSkip<UserMetaData>(x => x.Id);
            builder.WithSkip<UserMetaData>(x => x.User);

            // specify the custom faker for all objects that need special handling
            builder.WithOverride<UserMetaData>(_ => new UserMetaDataFaker());
        });

        // Generate fake data for a list of customers
        var userFaker = new UserFaker(Constants.EmailProvider);
        var users = userFaker.Generate(100);

        // Add the customers to the context and save changes
        _context.Users.AddRange(users);
        _ = await _context.SaveChangesAsync(new CancellationTokenSource().Token);
    }
}