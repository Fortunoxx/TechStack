namespace TechStack.Infrastructure;

using System.Reflection;
using Ardalis.GuardClauses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Infrastructure.Components.Activities;
using TechStack.Infrastructure.Components.Consumers;
using TechStack.Infrastructure.Components.Messaging;
using TechStack.Infrastructure.Data.Interceptors;
using TechStack.Infrastructure.Filter;
using TechStack.Infrastructure.Services;

public static class DependencyInjection
{
    private const string AssemblyNamespace = "TechStack";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // custom services
        services.AddSingleton<IEndpointNameFormatter>(new SnakeCaseEndpointNameFormatter(includeNamespace: true));
        services.AddSingleton<ILockService, LockService>();
        services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();

        services.AddMassTransit(options =>
        {
            options.AddConsumer<AddUserCommandConsumer>();
            options.AddConsumer<DeleteUserCommandConsumer>();
            options.AddConsumer<GetUserByIdQueryConsumer>();
            options.AddConsumer<GetAllUsersQueryConsumer>();
            options.AddConsumer<TestBusConsumer>();

            options.AddActivitiesFromNamespaceContaining<ActivityMarker>();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var assemblies = LoadAssemblies(baseDir, AssemblyNamespace);
            var rabbitMqOptionTypes = GetTypesFromBaseClass<MessageBrokerRabbitMqOption>(assemblies);

            options.UsingRabbitMq((context, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.UseKillSwitch(opt => opt
                    .SetActivationThreshold(3)
                    .SetTripThreshold(0.15)
                    .SetRestartTimeout(s: 10));

                busFactoryConfigurator.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(120)));

                busFactoryConfigurator.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), context);
                busFactoryConfigurator.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);

                busFactoryConfigurator.ConfigureEndpoints(context);

                foreach (var rabbitMqOptionType in rabbitMqOptionTypes)
                {
                    var methodInfo = rabbitMqOptionType.GetMethod(nameof(MessageBrokerRabbitMqOption.Configure));
                    var classInstance = Activator.CreateInstance(rabbitMqOptionType, null);

                    var parametersArray = new object[]
                    {
                        context, busFactoryConfigurator, configuration,
                    };

                    _ = methodInfo?.Invoke(classInstance, parametersArray);
                }
            });
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static List<Type> GetTypesFromBaseClass<T>(IList<Assembly> assemblies)
    {
        try
        {
            return assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsClass && x.BaseType == typeof(T)).ToList();
        }
        catch (ReflectionTypeLoadException)
        {
            throw;
        }
    }

    private static List<Assembly> LoadAssemblies(string appDirectory, string assemblyNamespace)
    {
        var assemblies = new List<Assembly>();
        var dllFiles = Directory.GetFiles(appDirectory, $"{assemblyNamespace}*.dll");
        foreach (var assemblyName in dllFiles.Select(AssemblyName.GetAssemblyName))
        {
            assemblies.Add(AppDomain.CurrentDomain.Load(assemblyName));
        }

        return assemblies;
    }
}
