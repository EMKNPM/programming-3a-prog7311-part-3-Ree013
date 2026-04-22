using Microsoft.EntityFrameworkCore;
using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Client)
            .WithMany(c => c.Contracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(s => s.Contract)
            .WithMany(c => c.ServiceRequests)
            .HasForeignKey(s => s.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Contract>()
            .Property(c => c.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.USDValue)
            .HasPrecision(18, 4);

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.ZARCost)
            .HasPrecision(18, 4);
    }
}