using CodingChallenge.Api.Data;
using CodingChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Api.Managers;

public interface IOrderManager
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<List<Order>> SearchAsync(string? term);
    Task<Order> AddAsync(Order order);
    Task<Order?> UpdateAsync(Order order);
    Task<bool> DeleteAsync(int id);

    /// <summary>Replaces the set of Boards included in the given Order.</summary>
    Task SetBoardsAsync(int orderId, IEnumerable<int> boardIds);
}

public class OrderManager : IOrderManager
{
    private readonly AppDbContext _context;

    public OrderManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderBoards).ThenInclude(ob => ob.Board)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderBoards).ThenInclude(ob => ob.Board)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> SearchAsync(string? term)
    {
        var query = _context.Orders.Include(o => o.OrderBoards).ThenInclude(ob => ob.Board).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(o => o.Name.Contains(term) || (o.Description != null && o.Description.Contains(term)));
        }

        return await query.ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> UpdateAsync(Order order)
    {
        var existing = await _context.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        if (existing is null)
        {
            return null;
        }

        existing.Name = order.Name;
        existing.Description = order.Description;
        existing.OrderDate = order.OrderDate;
        existing.Status = order.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (existing is null)
        {
            return false;
        }

        _context.Orders.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetBoardsAsync(int orderId, IEnumerable<int> boardIds)
    {
        var existingLinks = await _context.OrderBoards.Where(ob => ob.OrderId == orderId).ToListAsync();
        _context.OrderBoards.RemoveRange(existingLinks);

        foreach (var boardId in boardIds.Distinct())
        {
            _context.OrderBoards.Add(new OrderBoard { OrderId = orderId, BoardId = boardId });
        }

        await _context.SaveChangesAsync();
    }
}
