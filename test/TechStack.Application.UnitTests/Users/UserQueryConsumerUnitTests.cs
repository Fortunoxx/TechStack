namespace TechStack.Application.UnitTests;

using AutoFixture;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;
using TechStack.Application.Mappings;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;
using TechStack.Infrastructure;

[Trait("Category", "UnitTest")]
public class UserQueryConsumerUnitTests
{
    [Fact]
    public async Task GetAllUsersQueryConsumer_GetAllUsers_ShouldSuceedAsync()
    {
        // Arrange
        var dbContext = await GetDefaultApplicationDbContext();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<GetAllUsersQueryConsumer>();
            })
            .AddTransient(_ => dbContext)
            .AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>())
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<GetAllUsersQuery>();
        var query = new Fixture().Create<GetAllUsersQuery>();

        // Act
        var act = await cut.GetResponse<GetAllUsersQueryResult>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<GetAllUsersQuery>()).Should().BeTrue();
        (await testHarness.Sent.Any<GetAllUsersQueryResult>()).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserByIdQueryConsumer_GetUser_ShouldSuceedAsync()
    {
        // Arrange
        var dbContext = await GetDefaultApplicationDbContext();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<GetUserByIdQueryConsumer>();
            })
            .AddTransient(_ => dbContext)
            .AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>())
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<GetUserByIdQuery>();
        var query = new Fixture().Build<GetUserByIdQuery>().
            With(x => x.Id, dbContext.Users.First().Id).
            Create();

        // Act
        var act = await cut.GetResponse<GetUserByIdQueryResult>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<GetUserByIdQuery>()).Should().BeTrue();
        (await testHarness.Sent.Any<GetUserByIdQueryResult>()).Should().BeTrue();
        (await testHarness.Sent.Any<FaultedResponse>()).Should().BeFalse();
    }

    [Fact]
    public async Task GetUserByIdQueryConsumer_GetUnknownUser_ShouldFailAsync()
    {
        // Arrange
        var dbContext = await GetDefaultApplicationDbContext(false);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<GetUserByIdQueryConsumer>();
            })
            .AddTransient(_ => dbContext)
            .AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>())
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<GetUserByIdQuery>();
        var query = new Fixture().Create<GetUserByIdQuery>();

        // Act
        var act = await cut.GetResponse<FaultedResponse>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<GetUserByIdQuery>()).Should().BeTrue();
        (await testHarness.Sent.Any<GetUserByIdQueryResult>()).Should().BeFalse();
        (await testHarness.Sent.Any<FaultedResponse>()).Should().BeTrue();
    }

    private async static Task<IApplicationDbContext> GetDefaultApplicationDbContext(bool createUsers = true)
    {
        var dbContextBuilder = new DbContextOptionsBuilder<ApplicationDbContext>().
            UseInMemoryDatabase(databaseName: "TechStackUnitTestDb");

        var dbContext = new ApplicationDbContext(dbContextBuilder.Options);

        if (createUsers)
        {
            var users = new Fixture().
                Build<User>().
                Without(x => x.Id).
                CreateMany();
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync(new CancellationTokenSource().Token);
        }

        return dbContext;
    }
}