namespace TechStack.Application.UnitTests;

using AutoFixture;
using AwesomeAssertions;
using TechStack.Application.Users.Commands;

[Trait("Category", "UnitTest")]
public class ValidatorUnitTests
{
    [Fact]
    internal async Task AddUserCommandValidator_ValidateAsync_ShouldWorkAsync()
    {
        // Arrange 
        var fixture = new Fixture();
        var validator = new AddUserCommandValidator();

        var command = fixture.Create<AddUserCommand>();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    internal async Task DeleteUserCommandValidator_ValidateAsync_ShouldWorkAsync()
    {
        // Arrange 
        var fixture = new Fixture();
        var validator = new DeleteUserCommandValidator();

        var command = fixture.Create<DeleteUserCommand>();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}