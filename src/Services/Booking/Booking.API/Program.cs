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

app.MapGet("/api/bookings", () => Results.Ok(new[] { new { id = Guid.NewGuid(), scheduleId = Guid.NewGuid(), status = "Pending" } }))
   .WithName("GetBookings").WithOpenApi();

app.MapPost("/api/bookings", (CreateBookingRequest request) =>
{
    var id = Guid.NewGuid();
    return Results.Created($"/api/bookings/{id}", new { id, request.scheduleId, request.customerId, status = "Pending" });
}).WithName("CreateBooking").WithOpenApi();

app.Run();

public record CreateBookingRequest(Guid scheduleId, Guid customerId);
