namespace TechStack.Application.UnitTests;

using AutoFixture;
using FluentValidation.TestHelper;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;

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
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    internal async Task DeleteUserCommandValidator_ValidateAsync_ShouldWorkAsync()
    {
        // Arrange 
        var fixture = new Fixture();
        var validator = new DeleteUserCommandValidator();

        var command = fixture.Create<DeleteUserCommand>();

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    internal async Task GetUserByIdQueryValidator_ValidateAsync_ShouldWorkAsync()
    {
        // Arrange 
        var fixture = new Fixture();
        var validator = new GetUserByIdQueryValidator();

        var query = fixture.Create<GetUserByIdQuery>();

        // Act
        var result = await validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}