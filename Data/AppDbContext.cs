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
    public DbSet<VisitPayment> VisitPayments => Set<VisitPayment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<PayrollSettings> PayrollSettings => Set<PayrollSettings>();
    public DbSet<ManagerPayrollOverride> ManagerPayrollOverrides => Set<ManagerPayrollOverride>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Глобални филтри за Soft Delete
        b.Entity<Client>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Property>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Visit>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<DocumentTemplate>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<GeneratedDocument>().HasQueryFilter(x => !x.IsDeleted);

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

        b.Entity<VisitPayment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.FilePath).HasMaxLength(300);
            e.HasOne(x => x.Visit).WithMany(v => v.Payments).HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.VisitId);
            e.HasIndex(x => x.CreatedAtUtc);
        });

        b.Entity<AuditLog>(e =>
        {
            e.Property(x => x.UserEmail).HasMaxLength(200).IsRequired();
            e.Property(x => x.Action).HasMaxLength(50).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => new { x.EntityType, x.EntityId });
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AppSetting>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
            e.Property(x => x.Value).HasMaxLength(500).IsRequired();
            e.Property(x => x.Category).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();
            e.HasIndex(x => x.Category);
        });

        b.Entity<Expense>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.CreatedAtUtc);
            e.HasIndex(x => x.Category);
        });

        b.Entity<PayrollSettings>(e =>
        {
            e.Property(x => x.BaseSalary).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.CommissionPercentage).HasPrecision(5, 2).IsRequired();
            e.Property(x => x.VisitBonus).HasPrecision(18, 2).IsRequired();
        });

        b.Entity<ManagerPayrollOverride>(e =>
        {
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.BaseSalary).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.CommissionPercentage).HasPrecision(5, 2).IsRequired();
            e.Property(x => x.VisitBonus).HasPrecision(18, 2).IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.UserId).IsUnique();
        });

        b.Entity<DocumentTemplate>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100).IsRequired();
            e.Property(x => x.JsonSchema).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.FileTemplatePath).HasMaxLength(300);
            e.HasIndex(x => new { x.Category, x.IsActive });
            e.HasIndex(x => x.TemplateType);
        });

        b.Entity<GeneratedDocument>(e =>
        {
            e.Property(x => x.JsonData).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.FilePath).HasMaxLength(300).IsRequired();
            e.HasOne(x => x.DocumentTemplate)
                .WithMany(x => x.GeneratedDocuments)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.TemplateId);
            e.HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId });
            e.HasIndex(x => x.CreatedAtUtc);
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
