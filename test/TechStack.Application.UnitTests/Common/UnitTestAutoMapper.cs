namespace TechStack.Application.UnitTests.Common;

using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Internal;

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
