using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RealEstateCRM.Data;

public class DbInitializer : IDbInitializer
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IServiceProvider services, ILogger<DbInitializer> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using var scope = _services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();

        _logger.LogInformation("Applying migrations...");
        await db.Database.MigrateAsync(ct);

        _logger.LogInformation("Seeding Identity (roles/users)...");
        var (admin, manager, _) = await IdentitySeeder.SeedAsync(sp);

        _logger.LogInformation("Seeding domain data...");
        await DomainSeeder.SeedAsync(sp, admin.Id, manager.Id);

        _logger.LogInformation("DB initialization completed.");
    }
}
