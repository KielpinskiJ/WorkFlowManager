using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Models;

namespace WorkFlowManager.Data;

/// <summary>
/// Application database context inheriting from IdentityDbContext
/// to support ASP.NET Core Identity functionality.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }
    
    public DbSet<WorkShift> WorkShifts { get; set; }
    
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    
    public DbSet<Bonus> Bonuses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Department configuration
        builder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.Property(d => d.HourlyRate).HasColumnType("decimal(18,2)");
        });

        // ApplicationUser - Department relationship
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // WorkShift configuration
        builder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UserId).IsRequired();
            
            // WorkShift - ApplicationUser relationship
            entity.HasOne(s => s.User)
                .WithMany(u => u.Shifts)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // LeaveRequest configuration
        builder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.UserId).IsRequired();
            entity.Property(r => r.AdminComment).HasMaxLength(500);
            
            // LeaveRequest - ApplicationUser relationship
            entity.HasOne(r => r.User)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Index for faster queries on pending requests
            entity.HasIndex(r => r.Status);
        });
        
        // Bonus configuration
        builder.Entity<Bonus>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.UserId).IsRequired();
            entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");
            entity.Property(b => b.Reason).IsRequired().HasMaxLength(500);
            
            // Bonus - ApplicationUser relationship
            entity.HasOne(b => b.User)
                .WithMany(u => u.Bonuses)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Index for payroll calculations by date
            entity.HasIndex(b => b.DateGranted);
        });
    }
}
