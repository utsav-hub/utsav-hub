using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RABBITMQ_HOST"]
                   ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                   ?? "rabbitmq";
        var username = builder.Configuration["RABBITMQ_USERNAME"] ?? "guest";
        var password = builder.Configuration["RABBITMQ_PASSWORD"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.MapGet("/api/schedules", () =>
{
    var schedules = new[]
    {
        new { id = Guid.NewGuid(), route = "NYC-LON", departureUtc = DateTime.UtcNow.AddDays(1) }
    };
    return Results.Ok(schedules);
})
.WithName("GetSchedules")
.WithOpenApi();

app.MapPost("/api/schedules", (ScheduleRequest request) =>
{
    var createdId = Guid.NewGuid();
    return Results.Created($"/api/schedules/{createdId}", new { id = createdId, request.route, request.departureUtc });
})
.WithName("CreateSchedule")
.WithOpenApi();

app.Run();

public record ScheduleRequest(string route, DateTime departureUtc);
