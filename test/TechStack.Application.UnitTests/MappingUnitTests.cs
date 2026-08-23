namespace TechStack.Application.UnitTests;

using AutoFixture;
using AwesomeAssertions;
using Mapster;
using TechStack.Application.Common.Mappings;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

[Trait("Category", "UnitTest")]
public class MappingUnitTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void UserToGetUserByIdQueryResult_ShouldMapCorrectly()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        var mappingProfile = new UserMappingProfile();
        mappingProfile.Register(config);

        var user = _fixture.Build<User>()
            // .Without(u => u.DomainEvents)
            .With(x => x.MetaData,  _fixture.Build<UserMetaData>().Without(y => y.User).CreateMany(3).ToList())
            .Create();

        // Act
        var result = user.Adapt<GetUserByIdQueryResult>(config);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user, options => options.ExcludingMissingMembers().Excluding(x => x.Id));
    }

    [Fact]
    public void AddUserCommandToUser_ShouldMapCorrectly()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        var mappingProfile = new UserMappingProfile();
        mappingProfile.Register(config);

        var command = _fixture.Create<AddUserCommand>();

        // Act
        var mappedUser = command.Adapt<User>(config);

        // Assert
        mappedUser.Should().NotBeNull();
        mappedUser.Should().BeEquivalentTo(command, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public void GetUserByIdQueryResultToUser_ShouldMapCorrectly()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        var mappingProfile = new UserMappingProfile();
        mappingProfile.Register(config);

        var queryResult = _fixture.Create<GetUserByIdQueryResult>();

        // Act
        var mappedUser = queryResult.Adapt<User>(config);

        // Assert
        mappedUser.Should().NotBeNull();
        mappedUser.Should().BeEquivalentTo(queryResult, options => options.ExcludingMissingMembers().Excluding(x => x.Id));
    }
}