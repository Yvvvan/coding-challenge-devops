using CodingChallenge.Api.Data;
using CodingChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Api.Managers;

public interface IBoardManager
{
    Task<List<Board>> GetAllAsync();
    Task<Board?> GetByIdAsync(int id);
    Task<List<Board>> SearchAsync(string? term);
    Task<Board> AddAsync(Board board);
    Task<Board?> UpdateAsync(Board board);
    Task<bool> DeleteAsync(int id);

    /// <summary>Replaces the set of Components placed on the given Board.</summary>
    Task SetComponentsAsync(int boardId, IEnumerable<int> componentIds);
}

public class BoardManager : IBoardManager
{
    private readonly AppDbContext _context;

    public BoardManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Board>> GetAllAsync()
    {
        return await _context.Boards.AsNoTracking().ToListAsync();
    }

    public async Task<Board?> GetByIdAsync(int id)
    {
        return await _context.Boards
            .Include(b => b.OrderBoards).ThenInclude(ob => ob.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Board>> SearchAsync(string? term)
    {
        var query = _context.Boards.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(b => b.Name.Contains(term) || (b.Description != null && b.Description.Contains(term)));
        }

        return await query.ToListAsync();
    }

    public async Task<Board> AddAsync(Board board)
    {
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();
        return board;
    }

    public async Task<Board?> UpdateAsync(Board board)
    {
        var existing = await _context.Boards.FirstOrDefaultAsync(b => b.Id == board.Id);
        if (existing is null)
        {
            return null;
        }

        existing.Name = board.Name;
        existing.Description = board.Description;
        existing.Length = board.Length;
        existing.Width = board.Width;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null)
        {
            return false;
        }

        _context.Boards.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetComponentsAsync(int boardId, IEnumerable<int> componentIds)
    {
        var existingLinks = await _context.BoardComponents.Where(bc => bc.BoardId == boardId).ToListAsync();
        _context.BoardComponents.RemoveRange(existingLinks);

        foreach (var componentId in componentIds.Distinct())
        {
            _context.BoardComponents.Add(new BoardComponent { BoardId = boardId, ComponentId = componentId });
        }

        await _context.SaveChangesAsync();
    }
}
