namespace TechStack.Web.IntegrationTests.Users;

using AutoBogus;
using AutoBogus.Conventions;
using FluentAssertions;
using TechStack.Domain.Entities;
using TechStack.Infrastructure;
using TechStack.Web.IntegrationTests.Mocks;
using TechStack.Web.IntegrationTests.Fixtures;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

public sealed class UsersControllerIntegrationTests : IClassFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
    private readonly IntegrationTestFactory<Program, ApplicationDbContext> _factory;

    public UsersControllerIntegrationTests(IntegrationTestFactory<Program, ApplicationDbContext> factory)
    {
        _factory = factory;

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
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.AddRange(users);
        var code = context.SaveChanges();
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