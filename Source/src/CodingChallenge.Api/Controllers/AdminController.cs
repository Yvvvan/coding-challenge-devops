using CodingChallenge.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Explicit trigger to seed a 50,000+ component stress-test board (not run automatically on startup).</summary>
    [HttpPost("seed-large-dataset")]
    public async Task<IActionResult> SeedLargeDataset()
    {
        var seeded = await _adminService.SeedLargeDatasetAsync();
        return Ok(new { seeded });
    }
}
