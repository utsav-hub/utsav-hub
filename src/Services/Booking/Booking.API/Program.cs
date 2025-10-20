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
builder.Services.AddDbContext<BookingDbContext>(options =>
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
    x.AddRequestClient<ValidateSchedule>();
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
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.EnsureCreated();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.MapGet("/api/bookings", async (BookingDbContext db) =>
{
    var items = await db.Bookings.AsNoTracking().ToListAsync();
    return Results.Ok(items);
}).RequireAuthorization()
  .WithName("GetBookings").WithOpenApi();

app.MapPost("/api/bookings", async (CreateBookingRequest request, BookingDbContext db, IPublishEndpoint publishEndpoint, IRequestClient<ValidateSchedule> scheduleValidator) =>
{
    // validate schedule exists via RabbitMQ request/response
    var validationResponse = await scheduleValidator.GetResponse<ScheduleValidated>(new ValidateSchedule(request.scheduleId));
    if (!validationResponse.Message.Exists)
    {
        return Results.BadRequest(new { error = "Schedule not found" });
    }

    var booking = new Booking
    {
        Id = Guid.NewGuid(),
        ScheduleId = request.scheduleId,
        CustomerId = request.customerId,
        Status = "Pending",
        CreatedUtc = DateTime.UtcNow
    };
    db.Bookings.Add(booking);
    await db.SaveChangesAsync();

    await publishEndpoint.Publish(new Shared.Contracts.BookingCreated(booking.Id, booking.ScheduleId, booking.CustomerId, booking.CreatedUtc));

    return Results.Created($"/api/bookings/{booking.Id}", booking);
}).RequireAuthorization()
  .WithName("CreateBooking").WithOpenApi();

// minimal endpoint to issue JWT for testing purposes only
app.MapPost("/auth/token", (JwtTokenRequest req) =>
{
    var claims = new[] { new System.Security.Claims.Claim("sub", req.userId.ToString()) };
    var tokenDescriptor = new System.IdentityModel.Tokens.Jwt.SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["AccessTokenMinutes"] ?? "60")),
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
    };
    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var token = handler.CreateToken(tokenDescriptor);
    var jwt = handler.WriteToken(token);
    return Results.Ok(new { access_token = jwt, token_type = "Bearer" });
}).WithName("IssueToken").WithOpenApi();

app.Run();

public record CreateBookingRequest(Guid scheduleId, Guid customerId);
public record JwtTokenRequest(Guid userId);

public sealed class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }
    public DbSet<Booking> Bookings => Set<Booking>();
}

public sealed class Booking
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
