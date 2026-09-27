using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/boards")]
[RequireLogin]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BoardDto>>> GetAll([FromQuery] string? search)
    {
        var boards = string.IsNullOrWhiteSpace(search)
            ? await _boardService.GetAllAsync()
            : await _boardService.SearchAsync(search);

        return Ok(boards);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BoardDto>> GetById(int id)
    {
        var board = await _boardService.GetByIdAsync(id);
        return board is null ? NotFound() : Ok(board);
    }

    [HttpPost]
    public async Task<ActionResult<BoardDto>> Create([FromBody] BoardCreateDto dto)
    {
        var created = await _boardService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BoardDto>> Update(int id, [FromBody] BoardUpdateDto dto)
    {
        var updated = await _boardService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _boardService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (BoardInUseException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
