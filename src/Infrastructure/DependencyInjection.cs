namespace TechStack.Infrastructure;

using Ardalis.GuardClauses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Users.Queries;
using TechStack.Infrastructure.Consumers;
using TechStack.Infrastructure.Filter;
using TechStack.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // custom services
        services.AddSingleton<ILockService, LockService>();
        services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
        services.AddTransient<IApplicationDbContext, ApplicationDbContext>(); // ???

        services.AddMassTransit(options =>
        {
            options.AddConsumer<TestBusConsumer>();
            options.AddConsumer<GetUserByIdQueryConsumer>();

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

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            // options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}