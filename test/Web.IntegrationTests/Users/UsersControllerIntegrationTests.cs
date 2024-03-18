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
using AutoFixture;
using TechStack.Application.Users.Commands;
using System.Net.Mime;
using TechStack.Application.Common.Interfaces;

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
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        context.Users.AddRange(users);
        var code = context.SaveChanges();
    }

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
    
    [Fact(Skip = "TODO: Context is disposed - 500")]
    internal async Task UsersApi_CreatePerson_ShouldReturnValidResultAsync()
    {
        // Arrange 
        var cut = _factory.CreateClient();
        var user = new Fixture().Create<AddUserCommand>();
        
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
}