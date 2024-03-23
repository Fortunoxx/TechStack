namespace TechStack.Infrastructure;

using Ardalis.GuardClauses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Infrastructure.Consumers;
using TechStack.Infrastructure.Data.Interceptors;
using TechStack.Infrastructure.Filter;
using TechStack.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // custom services
        services.AddSingleton<ILockService, LockService>();
        services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();

        services.AddMassTransit(options =>
        {
            options.AddConsumer<AddUserCommandConsumer>();
            options.AddConsumer<DeleteUserCommandConsumer>();
            options.AddConsumer<GetUserByIdQueryConsumer>();
            options.AddConsumer<GetAllUsersQueryConsumer>();
            options.AddConsumer<TestBusConsumer>();

            options.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseKillSwitch(opt => opt
                    .SetActivationThreshold(3)
                    .SetTripThreshold(0.15)
                    .SetRestartTimeout(s: 10));

                cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(120)));

                cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), context);
                cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);

                cfg.ConfigureEndpoints(context);
            });
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        // services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}