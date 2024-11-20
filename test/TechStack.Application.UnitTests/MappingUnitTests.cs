namespace TechStack.Application.UnitTests;

using AutoFixture;
using AutoMapper;
using AutoMapper.Internal;
using FluentAssertions;
using TechStack.Application.Common.Mappings;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

public class AutoMapperUnitTestFixtureException(string message, IEnumerable<(Type TSource, Type TDestination)> untestedMappings) : Exception(message)
{
    public override string Message
    {
        get
        {
            if (untestedMappings != null)
            {
                var message = base.Message;
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}Found untested mappings. Please add tests for these mappings and verify";
                message += $"{Environment.NewLine}that you used {nameof(AutoMapperUnitTestFixture.AddTestedMapping)} to add them to the list of tested mappings.";
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}Untested Mappings:";
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}{string.Join(Environment.NewLine, untestedMappings.Select(x => $"{x.TSource} -> {x.TDestination}"))}";
                message += $"{Environment.NewLine}==================";
                return message;
            }

            return Message;
        }
    }
}

public class AutoMapperUnitTestFixture
{
    public HashSet<(Type Source, Type Destination)> TestedMappings { get; init; } = [];

    public void AddTestedMapping<TSrc, TDest>()
    {
        TestedMappings.Add((typeof(TSrc), typeof(TDest)));
    }

    public void AssertUnitTestCoverage(IMapper mapper)
    {
        var allMappings = mapper.ConfigurationProvider.Internal().GetAllTypeMaps();
        var untestedMappings = allMappings
            .Select(map => (map.SourceType, map.DestinationType))
            .Except(TestedMappings);

        if (untestedMappings.Any())
        {
            throw new AutoMapperUnitTestFixtureException("All mappings must be tested.", untestedMappings);
        }
    }
}

[Trait("Category", "UnitTest")]
public class MappingUnitTests
    : IClassFixture<AutoMapperUnitTestFixture>
{
    private readonly AutoMapperUnitTestFixture autoMapperUnitTestFixture;
    private readonly IMapper mapper;

    public MappingUnitTests(AutoMapperUnitTestFixture autoMapperUnitTestFixture)
    {
        this.autoMapperUnitTestFixture = autoMapperUnitTestFixture;

        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserMapping>());
        mapper = config.CreateMapper();
    }

    [Fact]
    public void AllMappingsShouldBeTested() => autoMapperUnitTestFixture.AssertUnitTestCoverage(mapper);

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
        // autoMapperUnitTestFixture.AddTestedMapping<User, GetUserByIdQueryResult>();

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
        autoMapperUnitTestFixture.AddTestedMapping<GetUserByIdQueryResult, User>();

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
        autoMapperUnitTestFixture.AddTestedMapping<AddUserCommand, User>();

        // Assert
        result.Should().BeEquivalentTo(addUserCommand);
    }
}