namespace TechStack.Infrastructure.UnitTests;

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Test.Commands;
using TechStack.Infrastructure.Components.Consumers;
using TechStack.Infrastructure.Services;
using Xunit;

[Trait("Category", "UnitTest")]
public class TestBusConsumerUnitTests
{
    [Fact]
    internal async Task TestBusConsumer_Command_ShouldRespondValidResultAsync()
    {
        // Arrange
        var distributedCache = Substitute.For<IDistributedCache>();
        var dictionary = new Dictionary<int, object>();
        var jsonString = JsonSerializer.Serialize(dictionary);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        distributedCache.GetAsync(Arg.Any<string>()).Returns(bytes);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<TestBusConsumer>();
            })
            .AddTransient(_ => new NullLogger<TestBusConsumer>())
            .AddTransient(_ => distributedCache)
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