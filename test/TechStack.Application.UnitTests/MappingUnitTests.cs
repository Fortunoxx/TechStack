namespace TechStack.Application.UnitTests;

using System.Linq;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using TechStack.Application.Common.Mappings;
using TechStack.Application.UnitTests.Common;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

[Trait("Category", "UnitTest")]
public class MappingUnitTests
{
    private readonly UnitTestAutoMapper mapper;

    public MappingUnitTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserMapping>());
        var mapper = config.CreateMapper();
        this.mapper = new UnitTestAutoMapper(mapper);
    }

    [Fact]
    public void AllMappingsShouldBeTested() => UnitTestAutoMapper.AssertUnitTestCoverage(mapper);

    [Fact]
    public void Application_Mapping_ShouldWorkAsync()
    {
        // Arrange 
        var cut = new MapperConfiguration(cfg => cfg.AddProfile<UserMapping>());

        // Act & Assert
        cut.AssertConfigurationIsValid();
    }

    [Fact]
    public void UserMapping_MapUserToGetUserByIdQueryResult_ShouldWorkAsync()
    {
        // Arrange
        var fixture = new Fixture();

        var meta = fixture.
            Build<UserMetaData>().
            Without(x => x.User).
            CreateMany().
            ToList();

        var user = fixture.
            Build<User>().
            With(x => x.MetaData, meta).
            Create();

        // Act
        var result = mapper.Map<GetUserByIdQueryResult>(user);

        // Assert
        result.Should().BeEquivalentTo(user, cfg => cfg.ExcludingMissingMembers());
    }

    [Fact]
    public void UserMapping_MapGetUserByIdQueryResultToUser_ShouldWorkAsync()
    {
        // Arrange
        var getUserByIdQueryResult = new Fixture().Create<GetUserByIdQueryResult>();

        // Act
        var result = mapper.Map<User>(getUserByIdQueryResult);

        // Assert
        result.Should().BeEquivalentTo(getUserByIdQueryResult);
    }

    [Fact]
    public void UserMapping_MapAddUserCommandToUser_ShouldWorkAsync()
    {
        // Arrange
        var addUserCommand = new Fixture().Create<AddUserCommand>();

        // Act
        var result = mapper.Map<User>(addUserCommand);

        // Assert
        result.Should().BeEquivalentTo(addUserCommand);
    }
}