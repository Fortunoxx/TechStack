namespace Application.UnitTests;

using AutoFixture;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Mappings;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

[Trait("Category", "UnitTest")]
public class UserQueryConsumerUnitTests
{

    [Fact]
    public async Task GetAllUsersQueryConsumer_GetAllUsers_ShouldSuceedAsync()
    {
        // Arrange
        var dbContext = Substitute.For<IApplicationDbContext>();
        var dbSetUser = Substitute.For<DbSet<User>, IQueryable<User>, IAsyncEnumerable<User>>();
        dbContext.Users.Returns(dbSetUser);

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
}