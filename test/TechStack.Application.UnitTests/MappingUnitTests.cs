namespace TechStack.Application.UnitTests;

using AutoFixture;
using AwesomeAssertions;
using TechStack.Application.Common.Mappings;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

[Trait("Category", "UnitTest")]
public class MappingUnitTests
{
    private readonly Fixture _fixture = new();
    private readonly UserMapper _mapper = new();

    [Fact]
    public void UserToGetUserByIdQueryResult_ShouldMapCorrectly()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(x => x.MetaData, _fixture.Build<UserMetaData>().Without(y => y.User).CreateMany(3).ToList())
            .Create();

        // Act
        var result = _mapper.UserToGetUserByIdQueryResult(user);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public void AddUserCommandToUser_ShouldMapCorrectly()
    {
        // Arrange
        var command = _fixture.Create<AddUserCommand>();

        // Act
        var mappedUser = _mapper.AddUserCommandToUser(command);

        // Assert
        mappedUser.Should().NotBeNull();
        mappedUser.Should().BeEquivalentTo(command, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public void GetUserByIdQueryResultToUser_ShouldMapCorrectly()
    {
        // Arrange
        var queryResult = _fixture.Create<GetUserByIdQueryResult>();

        // Act
        var mappedUser = _mapper.GetUserByIdQueryResultToUser(queryResult);

        // Assert
        mappedUser.Should().NotBeNull();
        mappedUser.Should().BeEquivalentTo(queryResult, options => options.ExcludingMissingMembers());
    }
}