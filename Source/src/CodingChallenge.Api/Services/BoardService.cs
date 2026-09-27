using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Api.Services;

public interface IBoardService
{
    Task<List<BoardDto>> GetAllAsync();
    Task<BoardDto?> GetByIdAsync(int id);
    Task<List<BoardDto>> SearchAsync(string? term);
    Task<BoardDto> CreateAsync(BoardCreateDto dto);
    Task<BoardDto?> UpdateAsync(int id, BoardUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public class BoardService : IBoardService
{
    private readonly IBoardManager _boardManager;
    private readonly IComponentManager _componentManager;
    private readonly ILogger<BoardService> _logger;

    public BoardService(IBoardManager boardManager, IComponentManager componentManager, ILogger<BoardService> logger)
    {
        _boardManager = boardManager;
        _componentManager = componentManager;
        _logger = logger;
    }

    public async Task<List<BoardDto>> GetAllAsync()
    {
        var boards = await _boardManager.GetAllAsync();
        return boards.Select(b => new BoardDto(b.Id, b.Name, b.Description, b.Length, b.Width, new List<OrderSummaryDto>(), new List<ComponentSummaryDto>())).ToList();
    }

    public async Task<BoardDto?> GetByIdAsync(int id)
    {
        var board = await _boardManager.GetByIdAsync(id);
        if (board is null)
        {
            return null;
        }

        var components = await _componentManager.GetByBoardIdAsync(id);
        return ToDto(board, components);
    }

    public async Task<List<BoardDto>> SearchAsync(string? term)
    {
        var boards = await _boardManager.SearchAsync(term);
        return boards.Select(b => new BoardDto(b.Id, b.Name, b.Description, b.Length, b.Width, new List<OrderSummaryDto>(), new List<ComponentSummaryDto>())).ToList();
    }

    public async Task<BoardDto> CreateAsync(BoardCreateDto dto)
    {
        var board = new Board
        {
            Name = dto.Name,
            Description = dto.Description,
            Length = dto.Length,
            Width = dto.Width
        };

        var created = await _boardManager.AddAsync(board);
        await _boardManager.SetComponentsAsync(created.Id, dto.ComponentIds);
        _logger.LogInformation("Created board {BoardId} ({BoardName})", created.Id, created.Name);

        var components = await _componentManager.GetByBoardIdAsync(created.Id);
        var reloaded = await _boardManager.GetByIdAsync(created.Id);
        return ToDto(reloaded!, components);
    }

    public async Task<BoardDto?> UpdateAsync(int id, BoardUpdateDto dto)
    {
        var board = new Board
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Length = dto.Length,
            Width = dto.Width
        };

        var updated = await _boardManager.UpdateAsync(board);
        if (updated is null)
        {
            return null;
        }

        await _boardManager.SetComponentsAsync(id, dto.ComponentIds);
        _logger.LogInformation("Updated board {BoardId}", id);

        var components = await _componentManager.GetByBoardIdAsync(id);
        var reloaded = await _boardManager.GetByIdAsync(id);
        return ToDto(reloaded!, components);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _boardManager.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Deleted board {BoardId}", id);
        }

        return deleted;
    }

    private static BoardDto ToDto(Board board, List<Component> components)
    {
        var orders = board.OrderBoards
            .Select(ob => new OrderSummaryDto(ob.Order.Id, ob.Order.Name, ob.Order.Status.ToString()))
            .ToList();

        var componentSummaries = components
            .Select(c => new ComponentSummaryDto(c.Id, c.Name, c.Quantity))
            .ToList();

        return new BoardDto(board.Id, board.Name, board.Description, board.Length, board.Width, orders, componentSummaries);
    }
}
