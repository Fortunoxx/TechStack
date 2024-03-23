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
        var command = new Fixture().Create<AddUserCommand>();

        // Act
        var act = await cut.GetResponse<AddUserCommandResponse>(command);

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
        var command = new Fixture().Create<AddUserCommand>();

        // Act
        var act = await cut.GetResponse<FaultedResponse>(command);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<AddUserCommand>()).Should().BeTrue();
        (await testHarness.Sent.Any<FaultedResponse>()).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserCommandConsumer_RemoveUser_ShouldSucceedAsync()
    {
        // Arrange
        var dbContext = Substitute.For<IApplicationDbContext>();
        var user = new Fixture().Create<User>();
        dbContext.Users.FindAsync(default).ReturnsForAnyArgs(user);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<DeleteUserCommandConsumer>();
            })
            .AddTransient(_ => dbContext)
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<DeleteUserCommand>();
        var query = new Fixture().Create<DeleteUserCommand>();

        // Act
        var act = await cut.GetResponse<AcceptedResponse>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<DeleteUserCommand>()).Should().BeTrue();
        (await testHarness.Sent.Any<AcceptedResponse>()).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserCommandConsumer_RemoveUserFallback_ShouldReturnFaultedResponsedAsync()
    {
        // Arrange
        var dbContext = Substitute.For<IApplicationDbContext>();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<DeleteUserCommandConsumer>();
            })
            .AddTransient(_ => dbContext)
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<DeleteUserCommand>();
        var command = new Fixture().Create<DeleteUserCommand>();

        // Act
        var act = await cut.GetResponse<FaultedResponse>(command);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<DeleteUserCommand>()).Should().BeTrue();
        (await testHarness.Sent.Any<FaultedResponse>()).Should().BeTrue();
    }
}