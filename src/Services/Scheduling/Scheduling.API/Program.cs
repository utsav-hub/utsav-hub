using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<SchedulingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
                           ?? builder.Configuration["ConnectionStrings:Default"]
                           ?? builder.Configuration["ConnectionStrings__Default"]
                           ?? "Host=localhost;Port=5432;Database=freight;Username=freight;Password=freight;";
    options.UseNpgsql(connectionString);
});

var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? builder.Configuration["JWT__Issuer"] ?? "freight";
var jwtAudience = builder.Configuration["JWT:Audience"] ?? builder.Configuration["JWT__Audience"] ?? "freight_clients";
var jwtSecret = builder.Configuration["JWT:Secret"] ?? builder.Configuration["JWT__Secret"] ?? "dev_only_secret_change_me";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<BookingCreatedConsumer>();

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

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.MapGet("/api/schedules", async (SchedulingDbContext db) =>
{
    var schedules = await db.Schedules.OrderByDescending(s => s.DepartureUtc).ToListAsync();
    return Results.Ok(schedules);
}).RequireAuthorization().WithName("GetSchedules").WithOpenApi();

app.MapPost("/api/schedules", async (ScheduleRequest request, SchedulingDbContext db) =>
{
    var entity = new Schedule
    {
        Id = Guid.NewGuid(),
        Route = request.route,
        DepartureUtc = request.departureUtc
    };
    db.Schedules.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/schedules/{entity.Id}", entity);
}).RequireAuthorization().WithName("CreateSchedule").WithOpenApi();

// Ensure database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public record ScheduleRequest(string route, DateTime departureUtc);

public class Schedule
{
    public Guid Id { get; set; }
    public string Route { get; set; } = string.Empty;
    public DateTime DepartureUtc { get; set; }
}

public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options) { }
    public DbSet<Schedule> Schedules => Set<Schedule>();
}

public class BookingCreatedConsumer : IConsumer<BookingCreated>
{
    private readonly SchedulingDbContext _db;

    public BookingCreatedConsumer(SchedulingDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<BookingCreated> context)
    {
        // Example: when a booking is created, we might adjust schedule capacity, etc.
        // For demo, just ensure schedule exists placeholder.
        var scheduleId = context.Message.ScheduleId;
        var schedule = await _db.Schedules.FindAsync(scheduleId);
        if (schedule == null)
        {
            // no-op in this demo
        }
    }
}
