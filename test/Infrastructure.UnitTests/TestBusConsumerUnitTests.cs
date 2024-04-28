namespace Infrastructure.UnitTests;

using AutoFixture;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Test.Commands;
using TechStack.Infrastructure.Components.Consumers;
using TechStack.Infrastructure.Services;
using Xunit;

public class TestBusConsumerUnitTests
{
    [Fact]
    internal async Task TestBusConsumer_Command_ShouldRespondValidResultAsync()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<TestBusConsumer>();
            })
            .AddTransient(_ => new NullLogger<TestBusConsumer>())
            .AddTransient<ILockService, LockService>()
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<TestCommand>();
        var command = new Fixture().Create<TestCommand>();

        // Act
        var act = cut.GetResponse<TestCommandResponse>(command);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<TestCommand>()).Should().BeTrue();
        (await testHarness.Sent.Any<TestCommandResponse>()).Should().BeTrue();
    }
}