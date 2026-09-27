using CodingChallenge.Api.Data;
using CodingChallenge.Api.Data.Seed;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Api.Services;

public interface IAdminService
{
    /// <summary>Seeds a single Board with 50,000+ Components. Idempotent.</summary>
    Task<bool> SeedLargeDatasetAsync();
}

/// <summary>Backs admin/dev-tool operations.
public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AppDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<bool> SeedLargeDatasetAsync()
    {
        _logger.LogInformation("Seeding large stress-test dataset...");
        var seeded = LargeDatasetSeeder.SeedIfMissing(_context);
        _logger.LogInformation(seeded ? "Large stress-test dataset seeded." : "Large stress-test dataset already present, skipped.");
        return Task.FromResult(seeded);
    }
}
