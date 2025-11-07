// Infrastructure/Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstateCRM.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Visit> Visits => Set<Visit>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Глобални филтри за Soft Delete
        b.Entity<Client>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Property>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Visit>().HasQueryFilter(x => !x.IsDeleted);

        // Индекси и ограничения
        b.Entity<Client>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Comments).HasMaxLength(1000);

            e.HasIndex(x => new { x.LastName, x.FirstName });
            e.HasOne(x => x.OwnerUser).WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Property>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(160).IsRequired();
            e.Property(x => x.Address).HasMaxLength(240).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2); // 18 digits total, 2 decimal places

            e.HasIndex(x => x.Status);
            e.HasIndex(x => new { x.Type, x.Price });
            e.HasOne(x => x.OwnerUser).WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SellerClient).WithMany().HasForeignKey(x => x.SellerClientId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Visit>(e =>
        {
            e.Property(x => x.VisitAtLocal).IsRequired();
            e.HasOne(x => x.Property).WithMany(p => p.Visits).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Client).WithMany(c => c.Visits).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.OwnerUser).WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.VisitAtLocal);
        });
    }

    // По желание: централизирано попълване на аудит полета
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        foreach (var e in ChangeTracker.Entries<BaseEntity>())
        {
            if (e.State == EntityState.Added)
                e.Entity.CreatedAtUtc = now;
            if (e.State == EntityState.Modified)
                e.Entity.ModifiedAtUtc = now;
        }
        return base.SaveChangesAsync(ct);
    }
}
