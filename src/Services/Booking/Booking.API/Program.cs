using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

// PostgreSQL EF Core
builder.Services.AddDbContext<BookingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
                           ?? builder.Configuration["ConnectionStrings:Default"]
                           ?? builder.Configuration["ConnectionStrings__Default"]
                           ?? "Host=localhost;Port=5432;Database=freight;Username=freight;Password=freight;";
    options.UseNpgsql(connectionString);
});

// JWT Auth
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

// MassTransit + RabbitMQ
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

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

// Token endpoint (demo only)
app.MapPost("/api/token", (string userId) =>
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { access_token = tokenString });
}).WithName("CreateToken").WithOpenApi();

// CRUD-like endpoints (secured)
app.MapGet("/api/bookings", async (BookingDbContext db) =>
{
    var bookings = await db.Bookings.OrderByDescending(b => b.CreatedUtc).ToListAsync();
    return Results.Ok(bookings);
}).RequireAuthorization().WithName("GetBookings").WithOpenApi();

app.MapPost("/api/bookings", async (CreateBookingRequest request, BookingDbContext db, IPublishEndpoint bus) =>
{
    var entity = new Booking
    {
        Id = Guid.NewGuid(),
        ScheduleId = request.scheduleId,
        CustomerId = request.customerId,
        Status = "Pending",
        CreatedUtc = DateTime.UtcNow
    };
    db.Bookings.Add(entity);
    await db.SaveChangesAsync();

    await bus.Publish(new BookingCreated(entity.Id, entity.ScheduleId, entity.CustomerId, entity.CreatedUtc));

    return Results.Created($"/api/bookings/{entity.Id}", entity);
}).RequireAuthorization().WithName("CreateBooking").WithOpenApi();

// Ensure database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public record CreateBookingRequest(Guid scheduleId, Guid customerId);

public class Booking
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }
    public DbSet<Booking> Bookings => Set<Booking>();
}
