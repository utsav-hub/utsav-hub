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

app.MapGet("/api/payments", () => Results.Ok(new[] { new { id = Guid.NewGuid(), bookingId = Guid.NewGuid(), amount = 100.0, currency = "USD", status = "Authorized" } }))
   .WithName("GetPayments").WithOpenApi();

app.MapPost("/api/payments", (CreatePaymentRequest request) =>
{
    var id = Guid.NewGuid();
    return Results.Created($"/api/payments/{id}", new { id, request.bookingId, request.amount, request.currency, status = "Authorized" });
}).WithName("CreatePayment").WithOpenApi();

app.Run();

public record CreatePaymentRequest(Guid bookingId, decimal amount, string currency);
