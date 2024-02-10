using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using TechStack.Infrastructure;
using TechStack.Application;
using TechStack.Web.Infrastructure;
using TechStack.Application.Common.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using TechStack.Application.Common.Validation;
using System.Text.Json;
using System.Buffers;
using TechStack.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddTransient(typeof(IValidationFailurePipe<>), typeof(ValidationFailurePipe<>));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAuthentication(opt => opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(serviceName: builder.Environment.ApplicationName)
    )
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter()
        .AddSource("MassTransit")
    )
    .WithMetrics(builder => builder
        .AddPrometheusExporter()
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
        .AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = [ 0, 0.005, 0.01, 0.025, 0.05,
                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 ]
            })
    );


var app = builder.Build();

app.AddCorrelationIdMiddleware();

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering(); // to enable set position
    await next();
});

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = LogHelper.CustomGetLevel;
    options.EnrichDiagnosticContext = async (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
        diagnosticContext.Set("Authorization", httpContext.Request.Headers.Authorization.FirstOrDefault());
        diagnosticContext.Set("CorrelationId", httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault());

        var requestBody = await GetRequestBody(httpContext.Request);
        if (!string.IsNullOrEmpty(requestBody))
        {
            diagnosticContext.Set("RequestBody", requestBody);
        }

        // var responseBody = await GetResponseBody(httpContext.Response);
        // if (!string.IsNullOrEmpty(requestBody))
        // {
        //     diagnosticContext.Set("ResponseBody", requestBody);
        // }
    };
});

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception is MassTransit.RequestException rex)
        {
            if (rex.InnerException is ValidationException vex)
            {
                context.Response.StatusCode = vex.StatusCode;
                var pd = new HttpValidationProblemDetails
                {
                    Detail = vex.Detail,
                    Errors = vex.Errors,
                    Instance = context.Request.Path,
                    Status = vex.StatusCode,
                    Type = typeof(ValidationException).FullName
                };

                await context.Response.WriteAsJsonAsync(pd);
                return;
            }
        }

        await context.Response.WriteAsJsonAsync(new { error = exception.Message });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Configure the Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();


async Task<string?> GetRequestBody(HttpRequest httpRequest)
{
    httpRequest.Body.Position = 0;
    var payload = await new StreamReader(httpRequest.Body).ReadToEndAsync();
  
    if (!string.IsNullOrEmpty(payload))
    {
        var json = JsonSerializer.Deserialize<object>(payload);
        // var json2 = JsonSerializer.Deserialize<UpsertLockCommand>(payload, new JsonSerializerOptions
        // {
        //     PropertyNameCaseInsensitive = true
        // });
        return $"{JsonSerializer.Serialize(json)} ";
    }

    return null;
}

async Task<string?> GetResponseBody(HttpResponse httpResponse)
{
    // httpResponse.Body.Position = 0;
    var payload = await new StreamReader(httpResponse.Body).ReadToEndAsync();

    if (!string.IsNullOrEmpty(payload))
    {
        var json = JsonSerializer.Deserialize<object>(payload);
        return $"{JsonSerializer.Serialize(json)} ";
    }

    return null;
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
