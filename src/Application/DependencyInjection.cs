namespace TechStack.Application;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MassTransit;
using TechStack.Application.Test.Queries;
using TechStack.Application.Common.Validation;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // services.AddMediatR(cfg => {
        //     cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        //     cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        //     cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        //     cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        //     cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        // });

        services.AddMediator(options =>
        {
            options.AddRequestClient<TestQuery>();
            options.AddConsumer<TestQueryMediatorConsumer>();
            options.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            // add validation filter here
            options.ConfigureMediator((context, mediator) => mediator.UseConsumeFilter(typeof(FluentValidationFilter<>), context));
        });

        // services.AddMassTransit(options =>
        // {
        //     options.AddConsumer<TestBusConsumer>();

        //     options.UsingRabbitMq((context, cfg) =>
        //     {
        //         cfg.UseKillSwitch(opt => opt
        //             .SetActivationThreshold(3)
        //             .SetTripThreshold(0.15)
        //             .SetRestartTimeout(s: 10));
        //         // cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
        //         cfg.ConfigureEndpoints(context);
        //     });
        // });


        return services;
    }
}