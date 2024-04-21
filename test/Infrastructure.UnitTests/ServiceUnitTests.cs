namespace Infrastructure.UnitTests;

using FluentAssertions;
using MassTransit;
using NSubstitute;
using TechStack.Infrastructure.Filter;
using TechStack.Infrastructure.Services;
using Xunit;

[Trait("Category", "UnitTest")]
public class ServiceUnitTests
{
    [Fact]
    internal Task CorrelationIdGenerator_SetValue_ShouldSucceedAsync()
    {
        // Arrange
        var cut = new CorrelationIdGenerator();
        var id = $"{Guid.NewGuid()}";

        // Act
        cut.Set(id);

        // Assert
        cut.Get().Should().Be(id);

        return Task.CompletedTask;
    }

    [Fact]
    internal Task LockService_CreateLock_ShouldSucceedAsync()
    {
        // Arrange
        var cut = new LockService();
        var data = Guid.NewGuid();
        var id = 100;

        // Act
        var act = cut.CreateLock(id, data);

        // Assert
        act.Should().BeTrue();
        cut.GetAllLocks().Should().NotBeNull().And.ContainKey(id);
        cut.GetById(id).Should().BeEquivalentTo(new KeyValuePair<int, object>(id, data));

        return Task.CompletedTask;
    }

    [Fact]
    internal Task LockService_DeleteExistingLock_ShouldSucceedAsync()
    {
        // Arrange
        var cut = new LockService();
        var data = Guid.NewGuid();
        var id = 100;

        // Act
        _ = cut.CreateLock(id, data);
        var act = cut.DeleteLock(id);

        // Assert
        act.Should().BeTrue();
        cut.GetAllLocks().Should().NotBeNull().And.HaveCount(0);

        return Task.CompletedTask;
    }

    [Fact]
    internal Task LockService_DeleteLock_ShouldFailAsync()
    {
        // Arrange
        var cut = new LockService();
        var id = 1;

        // Act
        var act = cut.DeleteLock(id);

        // Assert
        act.Should().BeFalse();
        cut.GetAllLocks().Should().NotBeNull().And.HaveCount(0);

        return Task.CompletedTask;
    }
}

[Trait("Category", "UnitTest")]
public class FilterUnitTests
{
    [Fact]
    internal async Task CorrelationIdPublishFilter_DeleteLock_ShouldFailAsync()
    {
        // Arrange
        var contextMock = Substitute.For<PublishContext<MockMessage>>();
        var publisherMock = Substitute.For<IPipe<PublishContext<MockMessage>>>();
        // var publisherMock = Substitute.For<IPublishEndpoint>();

        var filter = new CorrelationIdPublishFilter<MockMessage>(new CorrelationIdGenerator());

        // Set up test message
        var message = new MockMessage();

        // Act
        await filter.Send(contextMock, publisherMock);

        // Assert
        // Verify that the message publisher is called with the correct message
        // await publisherMock.Received(1).;

        // Alternatively, using FluentAssertions
        publisherMock.ReceivedCalls().Should().Contain(call =>
            call.GetMethodInfo().Name == nameof(IPublishEndpoint.Publish) &&
            (call.GetArguments()[0] as MockMessage) == message);
    }
}

public record MockMessage();