namespace TechStack.Application;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MassTransit;
using TechStack.Application.Test.Queries;
using TechStack.Application.Common.Validation;
using TechStack.Application.Common.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.AddRequestClient<TestQuery>();
            options.AddConsumer<TestQueryMediatorConsumer>();
            options.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            // add validation filter here
            options.ConfigureMediator((context, mediator) => mediator.UseConsumeFilter(typeof(FluentValidationFilter<>), context));
        });

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<IDataFetcherService, FirstDataFetcherService>();
        services.AddScoped<IDataFetcherService, DefaultDataFetcherService>();
        services.AddScoped<IAggregatorService, AggregatorService>();

        return services;
    }
}