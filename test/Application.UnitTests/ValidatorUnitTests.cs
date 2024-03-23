namespace Application.UnitTests;

using AutoFixture;
using FluentAssertions;
using TechStack.Application.Users.Queries;

[Trait("Category", "UnitTest")]
public class ValidatorUnitTests
{
    [Fact]
    public async Task Test()
    {
        // Arrage
        var cut = new GetUserByIdQueryValidator();
        var query = new Fixture().Build<GetUserByIdQuery>().With(x => x.Id, 0).Create();

        // Act
        var act = await cut.ValidateAsync(query);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().BeFalse();
    }

}