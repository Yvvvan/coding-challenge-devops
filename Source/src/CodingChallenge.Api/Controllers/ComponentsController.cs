using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/components")]
public class ComponentsController : ControllerBase
{
    private readonly IComponentService _componentService;

    public ComponentsController(IComponentService componentService)
    {
        _componentService = componentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ComponentDto>>> GetAll([FromQuery] string? search)
    {
        var components = string.IsNullOrWhiteSpace(search)
            ? await _componentService.GetAllAsync()
            : await _componentService.SearchAsync(search);

        return Ok(components);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<ComponentPageDto>> GetPage(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);

        var result = await _componentService.GetPageAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComponentDto>> GetById(int id)
    {
        var component = await _componentService.GetByIdAsync(id);
        return component is null ? NotFound() : Ok(component);
    }

    [HttpPost]
    public async Task<ActionResult<ComponentDto>> Create([FromBody] ComponentCreateDto dto)
    {
        var created = await _componentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ComponentDto>> Update(int id, [FromBody] ComponentUpdateDto dto)
    {
        var updated = await _componentService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _componentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
