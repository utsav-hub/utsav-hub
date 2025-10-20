using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// DbContext
builder.Services.AddDbContext<SchedulingDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// JWT Auth
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var key = jwtSection["Key"] ?? "your-very-strong-development-secret-change-me";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true
        };
    });

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<ValidateScheduleConsumer>();
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

app.UseAuthentication();
app.UseAuthorization();

// Ensure database exists (dev-only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    db.Database.EnsureCreated();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.MapGet("/api/schedules", async (SchedulingDbContext db) =>
{
    var items = await db.Schedules.AsNoTracking().ToListAsync();
    return Results.Ok(items);
})
.RequireAuthorization()
.WithName("GetSchedules")
.WithOpenApi();

// minimal endpoint to verify auth works
app.MapGet("/auth/ping", () => Results.Ok(new { ok = true, utc = DateTime.UtcNow }))
   .RequireAuthorization()
   .WithName("AuthPing").WithOpenApi();

app.MapPost("/api/schedules", async (ScheduleRequest request, SchedulingDbContext db, IPublishEndpoint publishEndpoint) =>
{
    var schedule = new Schedule
    {
        Id = Guid.NewGuid(),
        Route = request.route,
        DepartureUtc = request.departureUtc
    };
    db.Schedules.Add(schedule);
    await db.SaveChangesAsync();

    // Optional: publish an event for new schedule (not defined yet)
    // await publishEndpoint.Publish(new Shared.Contracts.ScheduleCreated(schedule.Id, schedule.Route, schedule.DepartureUtc));

    return Results.Created($"/api/schedules/{schedule.Id}", schedule);
})
.RequireAuthorization()
.WithName("CreateSchedule")
.WithOpenApi();

app.Run();

public record ScheduleRequest(string route, DateTime departureUtc);

public sealed class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options) { }
    public DbSet<Schedule> Schedules => Set<Schedule>();
}

public sealed class Schedule
{
    public Guid Id { get; set; }
    public string Route { get; set; } = string.Empty;
    public DateTime DepartureUtc { get; set; }
}

public sealed class ValidateScheduleConsumer : IConsumer<ValidateSchedule>
{
    private readonly SchedulingDbContext _db;
    public ValidateScheduleConsumer(SchedulingDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<ValidateSchedule> context)
    {
        var exists = await _db.Schedules.AsNoTracking().AnyAsync(s => s.Id == context.Message.ScheduleId);
        await context.RespondAsync(new ScheduleValidated(context.Message.ScheduleId, exists));
    }
}
