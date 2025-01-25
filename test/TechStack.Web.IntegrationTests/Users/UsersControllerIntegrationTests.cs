namespace TechStack.Web.IntegrationTests.Users;

using System.Net.Mime;
using Microsoft.Extensions.DependencyInjection;
using AutoBogus;
using AutoBogus.Conventions;
using AutoFixture;
using FluentAssertions;
using TechStack.Application.Users.Commands;
using TechStack.Domain.Entities;
using TechStack.Web.IntegrationTests.Mocks;
using TechStack.Web.IntegrationTests.Fixtures;
using TechStack.Infrastructure.Data;
using Xunit;
using TechStack.Web.IntegrationTests.Extensions;

[Collection(nameof(DatabaseCollectionSetup))]
public sealed class UsersControllerIntegrationTests(IntegrationTestFactory<Program, ApplicationDbContext> factory) : IAsyncLifetime
{
    private readonly IntegrationTestFactory<Program, ApplicationDbContext> _factory = factory;

    public async Task InitializeAsync()
    {
        var context = _factory.Services.CreateAsyncScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await SeedDatabaseAsync(context);
    }

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
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "user should be deleted");
        act.EnsureSuccessStatusCode();
    }

    private async Task SeedDatabaseAsync(ApplicationDbContext context)
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

            // exclude all navigation properties which would cause conflicts during insert
            builder
                .WithSkip<Badge>(x => x.Id)
                .WithSkip<Badge>(x => x.User)
                .WithSkip<Comment>(x => x.Post)
                .WithSkip<Comment>(x => x.User)
                .WithSkip<PostType>(x => x.Id)
                .WithSkip<Post>(x => x.OwnerUser)
                .WithSkip<Post>(x => x.LastEditorUser)
                .WithSkip<User>(x => x.Votes)
                .WithSkip<UserMetaData>(x => x.Id)
                .WithSkip<UserMetaData>(x => x.User)
                .WithSkip<Vote>(x => x.Post)
                .WithSkip<Vote>(x => x.User)
                .WithSkip<VoteType>(x => x.Id);

            // specify the custom faker for all objects that need special handling
            builder.WithOverride<UserMetaData>(_ => new UserMetaDataFaker());
            builder.WithOverride<Badge>(_ => new AutoFaker<Badge>().RuleFor(x => x.Name, faker => faker.Database.Random.Word()));
        });

        // Generate fake data for a list of customers
        var userFaker = new UserFaker(Constants.EmailProvider);
        var users = userFaker.Generate(10);

        var postId = 1;
        var postFaker = new AutoFaker<Post>(Constants.Locale)
            .RuleFor(x => x.Id, _ => postId++)
            .RuleFor(x => x.OwnerUserId, faker => faker.PickRandom(users).Id)
            .RuleFor(x => x.LastEditorUserId, faker => faker.PickRandom(users).Id);
        var posts = postFaker.Generate(100);

        var commentId = 1;
        var commentFaker = new AutoFaker<Comment>(Constants.Locale)
            .RuleFor(x => x.Id, _ => commentId++)
            .RuleFor(x => x.PostId, faker => faker.PickRandom(posts).Id)
            .RuleFor(x => x.UserId, faker => faker.PickRandom(users).Id);
        var comments = commentFaker.Generate(200);

        var voteId = 1;
        var voteFaker = new AutoFaker<Vote>(Constants.Locale)
            .RuleFor(x => x.Id, _ => voteId++)
            .RuleFor(x => x.PostId, faker => faker.PickRandom(posts).Id)
            .RuleFor(x => x.UserId, faker => faker.PickRandom(users).Id);
        var votes = voteFaker.Generate(300);

        // Add the users to the context and save changes
        context.Users.AddRange(users);
        await context.SaveChangesWithIdentityInsertAsync<User>();

        context.Posts.AddRange(posts);
        await context.SaveChangesWithIdentityInsertAsync<Post>();

        context.Comments.AddRange(comments);
        await context.SaveChangesWithIdentityInsertAsync<Comment>();

        context.Votes.AddRange(votes);
        await context.SaveChangesWithIdentityInsertAsync<Vote>();
    }
}
