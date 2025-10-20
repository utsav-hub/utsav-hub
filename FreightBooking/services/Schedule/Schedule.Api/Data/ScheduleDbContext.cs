using Microsoft.EntityFrameworkCore;
using Schedule.Api.Models;

namespace Schedule.Api.Data
{
    public class ScheduleDbContext : DbContext
    {
        public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : base(options)
        {
        }

        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleBooking> ScheduleBookings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Schedule entity
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RouteName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Origin).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
                entity.Property(e => e.VehicleType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.VehicleNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.DriverName).HasMaxLength(100);
                entity.Property(e => e.DriverContact).HasMaxLength(50);
                entity.Property(e => e.PricePerUnit).HasPrecision(18, 2);

                entity.HasMany(e => e.ScheduleBookings)
                      .WithOne(e => e.Schedule)
                      .HasForeignKey(e => e.ScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ScheduleBooking entity
            modelBuilder.Entity<ScheduleBooking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Seed data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@freightbooking.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
