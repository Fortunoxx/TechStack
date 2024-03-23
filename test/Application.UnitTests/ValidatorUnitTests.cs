namespace Application.UnitTests;

using AutoFixture;
using FluentAssertions;
using TechStack.Application.Users.Queries;

[Trait("Category", "UnitTest")]
public class ValidatorUnitTests
{
    [Theory]
    [InlineData("valid id", 1, true)]
    [InlineData("invalid id", -1, false)]
    public async Task GetUserByIdQueryValidator_DifferentIds_ShouldReturnExpectedResultAsync(string description, int id, bool expected)
    {
        // Arrage
        var cut = new GetUserByIdQueryValidator();
        var query = new Fixture().Build<GetUserByIdQuery>().With(x => x.Id, id).Create();

        // Act
        var act = await cut.ValidateAsync(query);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }
}