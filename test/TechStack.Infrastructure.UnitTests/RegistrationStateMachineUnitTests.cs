namespace TechStack.Infrastructure.UnitTests;

using AutoFixture;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Infrastructure.Components.StateMachines;
using TechStack.Infrastructure.Contracts;
using Xunit;

[Trait("Category", "UnitTest")]
public class RegistrationStateMachineUnitTests
{
    [Fact]
    internal async Task RegistrationStateMachine_RegistrationReceived_ShouldWorkAsync()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<RegistrationStateMachine, RegistrationState>();
                cfg.AddTransient(_ => GetDefaultConfiguration());
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();
        await harness.Start();

        var @event = new Fixture().Create<RegistrationReceived>();
        await harness.Bus.Publish(@event);

        (await harness.Consumed.Any<RegistrationReceived>()).Should().BeTrue();

        var sagaHarness = harness.GetSagaStateMachineHarness<RegistrationStateMachine, RegistrationState>();

        (await sagaHarness.Consumed.Any<RegistrationReceived>()).Should().BeTrue();
        (await sagaHarness.Created.Any(x => x.CorrelationId == @event.SubmissionId)).Should().BeTrue();

        var instance = sagaHarness.Created.ContainsInState(@event.SubmissionId, sagaHarness.StateMachine, sagaHarness.StateMachine.Received);

        instance.Should().NotBeNull("Saga instance not found");
        instance.CardNumber.Should().Be(@event.CardNumber);

        (await harness.Published.Any<ProcessRegistration>()).Should().BeTrue();
    }

    private static IConfiguration GetDefaultConfiguration()
    {
        var appSettingsStub = new List<KeyValuePair<string, string?>> {
            new ("RetryConfig:RetryCount", "1"),
            new ("RetryConfig:RetryDelay", "0")
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettingsStub)
            .Build();

        return configuration;
    }
}