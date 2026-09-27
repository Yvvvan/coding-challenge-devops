using System.Text.Json;
using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Api.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(int id);
    Task<List<OrderDto>> SearchAsync(string? term);
    Task<OrderDto> CreateAsync(OrderCreateDto dto);
    Task<OrderDto?> UpdateAsync(int id, OrderUpdateDto dto);
    Task<bool> DeleteAsync(int id);

    /// <summary>Serializes the Order (with its Boards and Components) to JSON, simulating a download to the production line.</summary>
    Task<(byte[] Content, string FileName)?> DownloadAsync(int id);
}

public class OrderService : IOrderService
{
    private readonly IOrderManager _orderManager;
    private readonly IBoardManager _boardManager;
    private readonly IComponentManager _componentManager;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderManager orderManager, IBoardManager boardManager, IComponentManager componentManager, ILogger<OrderService> logger)
    {
        _orderManager = orderManager;
        _boardManager = boardManager;
        _componentManager = componentManager;
        _logger = logger;
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _orderManager.GetAllAsync();
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderManager.GetByIdAsync(id);
        return order is null ? null : ToDto(order);
    }

    public async Task<List<OrderDto>> SearchAsync(string? term)
    {
        var orders = await _orderManager.SearchAsync(term);
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto> CreateAsync(OrderCreateDto dto)
    {
        var order = new Order
        {
            Name = dto.Name,
            Description = dto.Description,
            OrderDate = dto.OrderDate,
            Status = OrderStatus.Active
        };

        var created = await _orderManager.AddAsync(order);
        await _orderManager.SetBoardsAsync(created.Id, dto.BoardIds);
        _logger.LogInformation("Created order {OrderId} ({OrderName})", created.Id, created.Name);

        var reloaded = await _orderManager.GetByIdAsync(created.Id);
        return ToDto(reloaded!);
    }

    public async Task<OrderDto?> UpdateAsync(int id, OrderUpdateDto dto)
    {
        var existing = await _orderManager.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        var order = new Order
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            OrderDate = dto.OrderDate,
            Status = existing.Status
        };

        var updated = await _orderManager.UpdateAsync(order);
        if (updated is null)
        {
            return null;
        }

        await _orderManager.SetBoardsAsync(id, dto.BoardIds);
        _logger.LogInformation("Updated order {OrderId}", id);

        var reloaded = await _orderManager.GetByIdAsync(id);
        return ToDto(reloaded!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _orderManager.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Deleted order {OrderId}", id);
        }

        return deleted;
    }

    public async Task<(byte[] Content, string FileName)?> DownloadAsync(int id)
    {
        var order = await _orderManager.GetByIdAsync(id);
        if (order is null)
        {
            return null;
        }

        var boardExports = new List<BoardExportDto>();
        foreach (var orderBoard in order.OrderBoards)
        {
            var components = await _componentManager.GetByBoardIdAsync(orderBoard.BoardId);
            boardExports.Add(new BoardExportDto(
                orderBoard.Board.Id,
                orderBoard.Board.Name,
                orderBoard.Board.Description,
                orderBoard.Board.Length,
                orderBoard.Board.Width,
                components.Select(c => new ComponentSummaryDto(c.Id, c.Name, c.Quantity)).ToList()));
        }

        var export = new OrderExportDto(order.Id, order.Name, order.Description, order.OrderDate, boardExports);
        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        order.Status = OrderStatus.Downloaded;
        await _orderManager.UpdateAsync(order);
        _logger.LogInformation("Downloaded order {OrderId} to production line ({ByteCount} bytes)", id, bytes.Length);

        var fileName = $"order-{order.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        return (bytes, fileName);
    }

    private static OrderDto ToDto(Order order)
    {
        var boards = order.OrderBoards
            .Select(ob => new BoardSummaryDto(ob.Board.Id, ob.Board.Name, ob.Board.Length, ob.Board.Width))
            .ToList();

        return new OrderDto(order.Id, order.Name, order.Description, order.OrderDate, order.Status.ToString(), boards);
    }
}
