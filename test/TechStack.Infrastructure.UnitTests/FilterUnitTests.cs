namespace TechStack.Infrastructure.UnitTests;

using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;
using TechStack.Infrastructure.Filter;
using TechStack.Infrastructure.UnitTests.Mocks;
using Xunit;

[Trait("Category", "UnitTest")]
public class FilterUnitTests
{
    [Fact]
    internal async Task PublishFilter_InBus_ShouldBeCalled()
    {
        // Arrange
        var correlationIdGeneratorMock = Substitute.For<ICorrelationIdGenerator>();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), context);

                    cfg.ConfigureEndpoints(context);
                });
                x.AddScoped(_ => correlationIdGeneratorMock);
                x.AddConsumer<MockMessageConsumer>();
            })
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();
        testHarness.TestTimeout = TimeSpan.FromSeconds(3);
        await testHarness.Start();

        var message = new MockMessage();
        var cut = testHarness.GetRequestClient<MockMessage>();

        // Act
        var act = await cut.GetResponse<AcceptedResponse>(message);

        // Assert
        (await testHarness.Published.Any<MockMessage>()).Should().BeTrue();
    }

    [Fact]
    internal async Task CorrelationIdPublishFilter_Send_BeCalledAndWork()
    {
        // Arrange
        var correlationIdGenerator = Substitute.For<ICorrelationIdGenerator>();
        var publishContext = Substitute.For<IPipe<PublishContext<MockMessage>>>();
        var sendContext = Substitute.For<PublishContext<MockMessage>>();

        var cut = new CorrelationIdPublishFilter<MockMessage>(correlationIdGenerator);

        correlationIdGenerator.Get().Returns($"{NewId.NextGuid()}");

        // Act
        await cut.Send(sendContext, publishContext);

        // Assert
        correlationIdGenerator.Received(1);
        publishContext.Received(1);
        sendContext.Headers.Should().NotBeNull();
    }

    [Fact]
    internal async Task CorrelationIdConsumeFilter_Consume_ShouldBeCalledAndWork()
    {
        // Arrange
        var pipeConsumeContext = Substitute.For<IPipe<ConsumeContext<MockMessage>>>();
        var consumeContext = Substitute.For<ConsumeContext<MockMessage>>();
        var logger = new NullLogger<CorrelationIdConsumeFilter<MockMessage>>();

        var cut = new CorrelationIdConsumeFilter<MockMessage>(logger);
        var headerValue = new HeaderValue("X-Correlation-Id", "test");
        _ = consumeContext.Headers.Append(headerValue);

        // Act
        await cut.Send(consumeContext, pipeConsumeContext);

        // Assert
        pipeConsumeContext.Received(1);
        consumeContext.Headers.Should().NotBeNull();
    }
}
