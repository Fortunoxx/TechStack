namespace TechStack.Application.UnitTests;

using System.Linq;
using System.Linq.Expressions;
using AutoFixture;
using AutoMapper;
using AutoMapper.Internal;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using TechStack.Application.Common.Mappings;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

public class UnitTestAutoMapperException(string message, IEnumerable<(Type TSource, Type TDestination)> untestedMappings) : Exception(message)
{
    public override string Message
    {
        get
        {
            if (untestedMappings != null)
            {
                var message = base.Message;
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}Found untested mappings. Please add unit tests for these mappings and make sure to use {nameof(UnitTestAutoMapper)}";
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

public class UnitTestAutoMapper(IMapper mapper) : IMapper
{
    public static HashSet<(Type Source, Type Destination)> TestedMappings { get; } = [];

    public static void AddTestedMapping<TSrc, TDest>()
    {
        TestedMappings.Add((typeof(TSrc), typeof(TDest)));
    }

    public static void AddTestedMapping<TDest>(object source)
    {
        TestedMappings.Add((source.GetType(), typeof(TDest)));
    }

    public static void AssertUnitTestCoverage(IMapper mapper)
    {
        var allMappings = mapper.ConfigurationProvider.Internal().GetAllTypeMaps();
        var untestedMappings = allMappings
            .Select(map => (map.SourceType, map.DestinationType))
            .Except(TestedMappings);

        if (untestedMappings.Any())
        {
            throw new UnitTestAutoMapperException("All mappings must be tested.", untestedMappings);
        }
    }

    public IConfigurationProvider ConfigurationProvider => mapper.ConfigurationProvider;

    public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions<object, TDestination>> opts)
    {
        return mapper.Map(source, opts);
    }

    public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
    {
        return mapper.Map(source, opts);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
    {
        return mapper.Map(source, destination, opts);
    }

    public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions<object, object>> opts)
    {
        return mapper.Map(source, sourceType, destinationType, opts);
    }

    public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions<object, object>> opts)
    {
        return mapper.Map(source, destination, sourceType, destinationType, opts);
    }

    public TDestination Map<TDestination>(object source)
    {
        AddTestedMapping<TDestination>(source);
        return mapper.Map<TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return mapper.Map<TSource, TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return mapper.Map(source, destination);
    }

    public object Map(object source, Type sourceType, Type destinationType)
    {
        return mapper.Map(source, sourceType, destinationType);
    }

    public object Map(object source, object destination, Type sourceType, Type destinationType)
    {
        return mapper.Map(source, destination, sourceType, destinationType);
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand)
    {
        return mapper.ProjectTo(source, parameters, membersToExpand);
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
    {
        return mapper.ProjectTo<TDestination>(source, parameters, membersToExpand);
    }

    public IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters = null, params string[] membersToExpand)
    {
        return mapper.ProjectTo(source, destinationType, parameters, membersToExpand);
    }
}

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