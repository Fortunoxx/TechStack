namespace Application.UnitTests;

using AutoFixture;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;
using TechStack.Application.Mappings;
using TechStack.Application.Users.Commands;
using TechStack.Domain.Entities;

[Trait("Category", "UnitTest")]
public class UserCommandConsumerUnitTests
{
    [Fact]
    public async Task AddUserCommandConsumer_AddUser_ShouldSucceedAsync()
    {
        // Arrange
        var dbContext = Substitute.For<IApplicationDbContext>();
        var fakeUsers = Substitute.For<DbSet<User>>();
        dbContext.Users.Returns(fakeUsers);
        dbContext.SaveChangesAsync(default).ReturnsForAnyArgs(1);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<AddUserCommandConsumer>();
            })
            .AddTransient(_ => dbContext)
            .AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>())
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<AddUserCommand>();
        var query = new Fixture().Create<AddUserCommand>();

        // Act
        var act = await cut.GetResponse<AddUserCommandResponse>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<AddUserCommand>()).Should().BeTrue();
        (await testHarness.Sent.Any<AddUserCommandResponse>()).Should().BeTrue();
    }

    [Fact]
    public async Task AddUserCommandConsumer_AddUserFallback_ShouldReturnFaultedResponsedAsync()
    {
        // Arrange
        var dbContext = Substitute.For<IApplicationDbContext>();
        var fakeUsers = Substitute.For<DbSet<User>>();
        dbContext.Users.Returns(fakeUsers);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<AddUserCommandConsumer>();
            })
            .AddTransient(_ => dbContext)
            .AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>())
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<AddUserCommand>();
        var query = new Fixture().Create<AddUserCommand>();

        // Act
        var act = await cut.GetResponse<FaultedResponse>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<AddUserCommand>()).Should().BeTrue();
        (await testHarness.Sent.Any<FaultedResponse>()).Should().BeTrue();
    }
}