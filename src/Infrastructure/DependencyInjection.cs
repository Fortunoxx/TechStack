namespace TechStack.Infrastructure;

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;
using TechStack.Infrastructure.Consumers;
using TechStack.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(options =>
        {
            options.AddConsumersFromNamespaceContaining<TestConsumer>();
            options.AddRequestClient<TestQuery>();

            options.UsingRabbitMq((context, cfg) =>
            {
                // cfg.UseMessageRetry(opt => opt.Exponential(7, TimeSpan.FromMilliseconds(300), TimeSpan.FromMinutes(120), TimeSpan.FromMilliseconds(300)));
                cfg.UseKillSwitch(opt => opt
                    .SetActivationThreshold(3)
                    .SetTripThreshold(0.15)
                    .SetRestartTimeout(s: 10));
                // cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                cfg.ConfigureEndpoints(context);
            });
        });

        // custom services
        services.AddSingleton<ILockService, LockService>();

        return services;
    }
}