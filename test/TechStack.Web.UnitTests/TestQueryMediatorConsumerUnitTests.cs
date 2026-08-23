namespace TechStack.Web.UnitTests;

using System.Threading.Tasks;
using AutoFixture;
using AwesomeAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Test.Queries;
using Xunit;

[Trait("Category", "UnitTest")]
public class TestQueryMediatorConsumerUnitTests
{
    [Fact]
    internal async Task TestQueryMediatorConsumer_TestQuery_ShouldRespondValidResultAsync()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<TestQueryMediatorConsumer>();
            })
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();

        await testHarness.Start();

        var cut = testHarness.GetRequestClient<TestQuery>();
        var query = new Fixture().Create<TestQuery>();

        // Act
        var act = cut.GetResponse<TestQueryResult>(query);

        // Assert
        act.Should().NotBeNull();
        (await testHarness.Consumed.Any<TestQuery>()).Should().BeTrue();
        (await testHarness.Sent.Any<TestQueryResult>()).Should().BeTrue();
    }
}
