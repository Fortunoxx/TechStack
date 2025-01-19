namespace TechStack.Application.UnitTests;

using FluentValidation;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Common.Validation;

[Trait("Category", "UnitTest")]
public class FluentValidationFilterTest
{
    [Fact]
    internal async Task Consumer_WithFilter_ShouldWorkAsync()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => cfg.UsingInMemory((context, mem) =>
            {
                cfg.AddConsumer<MyTestMessageMockConsumer>();
                cfg.AddValidatorsFromAssemblyContaining<MyTestMessageMockValidator>();
                mem.UseConsumeFilter(typeof(FluentValidationFilter<>), context);
            }))
            .BuildServiceProvider(true);

        var testHarness = provider.GetTestHarness();
        await testHarness.Start();

        var cut = testHarness.GetRequestClient<MyTestMessage>();

        // Act
        var act = await cut.GetResponse<MyTestResponse>(new MyTestMessage());

        // Assert
        act.Should().NotBeNull();
    }

    internal class MyTestMessageMockValidator : AbstractValidator<MyTestMessage>
    {
        public MyTestMessageMockValidator()
            => RuleFor(x => x).NotNull();
    }

    internal record MyTestMessage;
    internal record MyTestResponse;

    internal class MyTestMessageMockConsumer : IConsumer<MyTestMessage>
    {
        public async Task Consume(ConsumeContext<MyTestMessage> context)
            => await context.RespondAsync(new MyTestResponse());
    }
}