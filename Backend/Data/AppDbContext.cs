using CutiApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CutiApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Password).IsRequired().HasMaxLength(100);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(lr => lr.Id);
            entity.Property(lr => lr.Reason).IsRequired().HasMaxLength(500);
            entity.Property(lr => lr.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(lr => lr.CreatedAt).IsRequired();

            entity.HasOne(lr => lr.User)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(lr => lr.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(lb => lb.Id);
            entity.HasIndex(lb => lb.UserId).IsUnique();
            entity.Property(lb => lb.RemainingDays).IsRequired();

            entity.HasOne(lb => lb.User)
                .WithOne(u => u.LeaveBalance)
                .HasForeignKey<LeaveBalance>(lb => lb.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "budi.santoso",
                Password = "budi123",
                FullName = "Budi Santoso",
                Role = "Manager"
            },
            new User
            {
                Id = 2,
                Username = "siti.aminah",
                Password = "siti123",
                FullName = "Siti Aminah",
                Role = "Employee"
            },
            new User
            {
                Id = 3,
                Username = "andi.wijaya",
                Password = "andi123",
                FullName = "Andi Wijaya",
                Role = "Employee"
            }
        );

        modelBuilder.Entity<LeaveBalance>().HasData(
            new LeaveBalance { Id = 1, UserId = 2, RemainingDays = 12 },
            new LeaveBalance { Id = 2, UserId = 3, RemainingDays = 12 }
        );
    }
}

