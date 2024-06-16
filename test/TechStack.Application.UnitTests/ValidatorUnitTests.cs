namespace TechStack.Application.UnitTests;

using AutoFixture;
using FluentAssertions;
using TechStack.Application.Test.Commands;
using TechStack.Application.Test.Queries;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;

[Trait("Category", "UnitTest")]
public class ValidatorUnitTests
{
    [Theory]
    [InlineData("Valid Object", 0, true)]
    [InlineData("Invalid DisplayName", 1, false)]
    [InlineData("Invalid DownVotes", 2, false)]
    [InlineData("Invalid LastAccessDate", 3, false)]
    [InlineData("Invalid Reputation", 4, false)]
    [InlineData("Invalid UpVotes", 5, false)]
    [InlineData("Invalid Views", 6, false)]
    public async Task AddUserCommandValidator_DifferentScenarios_ShouldReturnExpectedResultAsync(string description, int id, bool expected)
    {
        // Arrage
        var cut = new AddUserCommandValidator();
        var builder = new Fixture().Build<AddUserCommand>();
        var msg = id switch
        {
            1 => builder.With(x => x.DisplayName, string.Empty),
            2 => builder.With(x => x.DownVotes, -1),
            3 => builder.With(x => x.LastAccessDate, (DateTime?)null),
            4 => builder.With(x => x.Reputation, -1),
            5 => builder.With(x => x.UpVotes, -1),
            6 => builder.With(x => x.Views, -1),
            _ => builder,
        };

        // Act
        var act = await cut.ValidateAsync(msg.Create());

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }
    
    [Theory]
    [InlineData("valid id", 1, true)]
    [InlineData("invalid id", 0, false)]
    [InlineData("invalid id", -1, false)]
    public async Task AlterUserCommandValidator_DifferentIds_ShouldReturnExpectedResultAsync(string description, int id, bool expected)
    {
        // Arrage
        var cut = new AlterUserCommandValidator();
        var cmd = new Fixture().
            Build<AlterUserCommand>().
            With(x => x.Id, id).
            Create();

        // Act
        var act = await cut.ValidateAsync(cmd);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }

    [Theory]
    [InlineData(0, "Valid Object", true)]
    [InlineData(1, "No DisplayName", false)]
    [InlineData(2, "Invalid DownVotes", false)]
    [InlineData(3, "No DownVotes", false)]
    [InlineData(4, "No LastAccessDate", false)]
    [InlineData(5, "Invalid Reputation", false)]
    [InlineData(6, "No Reputation", false)]
    [InlineData(7, "Invalid UpVotes", false)]
    [InlineData(8, "No UpVotes", false)]
    [InlineData(9, "Invalid Views", false)]
    [InlineData(10, "No Views", false)]
    public async Task AlterUserCommandPartValidator_DifferentScenarios_ShouldBeExpectedResultAsync(short scenario, string description, bool expected)
    {
        // Arrage
        var cut = new AlterUserCommandPartValidator();
        var builder = new Fixture().
            Build<AlterUserCommandPart>();
            
        var cmd = scenario switch
        {
            1 => builder.Without(x => x.DisplayName).Create(),
            2 => builder.With(x => x.DownVotes, -1).Create(),
            3 => builder.Without(x => x.DownVotes).Create(),
            4 => builder.Without(x => x.LastAccessDate).Create(),
            5 => builder.With(x => x.Reputation, -1).Create(),
            6 => builder.Without(x => x.Reputation).Create(),
            7 => builder.With(x => x.UpVotes, -1).Create(),
            8 => builder.Without(x => x.UpVotes).Create(),
            9 => builder.With(x => x.Views, -1).Create(),
            10 => builder.Without(x => x.Views).Create(),
            _ => builder.Create(),
        };

        // Act
        var act = await cut.ValidateAsync(cmd);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }
    
    [Theory]
    [InlineData("valid id", 1, true)]
    [InlineData("invalid id", 0, false)]
    [InlineData("invalid id", -1, false)]
    public async Task DeleteUserCommandValidator_DifferentIds_ShouldReturnExpectedResultAsync(string description, int id, bool expected)
    {
        // Arrage
        var cut = new DeleteUserCommandValidator();
        var command = new Fixture().Build<DeleteUserCommand>().With(x => x.Id, id).Create();

        // Act
        var act = await cut.ValidateAsync(command);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }

    [Theory]
    [InlineData("valid id", 1, true)]
    [InlineData("invalid id", 0, false)]
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

    [Theory]
    [InlineData("valid id", 1, true)]
    [InlineData("invalid id", 0, false)]
    [InlineData("invalid id", -1, false)]
    public async Task TestCommandValidator_DifferentIds_ShouldReturnExpectedResultAsync(string description, int id, bool expected)
    {
        // Arrage
        var cut = new TestCommandValidator();
        var command = new Fixture().Build<TestCommand>().With(x => x.Id, id).Create();

        // Act
        var act = await cut.ValidateAsync(command);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }

    [Theory]
    [InlineData("valid id", 1, true)]
    [InlineData("invalid id", 0, false)]
    [InlineData("invalid id", -1, false)]
    public async Task TestQueryValidator_DifferentIds_ShouldReturnExpectedResultAsync(string description, int id, bool expected)
    {
        // Arrage
        var cut = new TestQueryValidator();
        var query = new Fixture().Build<TestQuery>().With(x => x.Id, id).Create();

        // Act
        var act = await cut.ValidateAsync(query);

        // Assert
        act.Should().NotBeNull();
        act.IsValid.Should().Be(expected, description);
    }
}