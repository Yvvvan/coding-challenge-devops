using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/orders")]
[RequireLogin]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAll([FromQuery] string? search)
    {
        var orders = string.IsNullOrWhiteSpace(search)
            ? await _orderService.GetAllAsync()
            : await _orderService.SearchAsync(search);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] OrderCreateDto dto)
    {
        var created = await _orderService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] OrderUpdateDto dto)
    {
        var updated = await _orderService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _orderService.DownloadAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return File(result.Value.Content, "application/json", result.Value.FileName);
    }
}
