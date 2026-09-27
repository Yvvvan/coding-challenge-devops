using CodingChallenge.Api.Data;
using CodingChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Api.Managers;

public interface IComponentManager
{
    Task<List<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
    Task<List<Component>> SearchAsync(string? term);
    Task<Component> AddAsync(Component component);
    Task<Component?> UpdateAsync(Component component);
    Task<bool> DeleteAsync(int id);

    /// <summary>Returns all Components placed on the given Board.</summary>
    Task<List<Component>> GetByBoardIdAsync(int boardId);
}

public class ComponentManager : IComponentManager
{
    private readonly AppDbContext _context;

    public ComponentManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Component>> GetAllAsync()
    {
        return await _context.Components.AsNoTracking().ToListAsync();
    }

    public async Task<Component?> GetByIdAsync(int id)
    {
        return await _context.Components
            .Include(c => c.BoardComponents).ThenInclude(bc => bc.Board)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Component>> SearchAsync(string? term)
    {
        var query = _context.Components.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(c => c.Name.Contains(term) || (c.Description != null && c.Description.Contains(term)));
        }

        return await query.ToListAsync();
    }

    public async Task<Component> AddAsync(Component component)
    {
        _context.Components.Add(component);
        await _context.SaveChangesAsync();
        return component;
    }

    public async Task<Component?> UpdateAsync(Component component)
    {
        var existing = await _context.Components.FirstOrDefaultAsync(c => c.Id == component.Id);
        if (existing is null)
        {
            return null;
        }

        existing.Name = component.Name;
        existing.Description = component.Description;
        existing.Quantity = component.Quantity;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
        {
            return false;
        }

        _context.Components.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Component>> GetByBoardIdAsync(int boardId)
    {
        // Components carry their board links via BoardComponents, so pull the catalog with links attached and match up in-memory.
        var allComponents = await _context.Components
            .Include(c => c.BoardComponents)
            .ToListAsync();

        return allComponents
            .Where(c => c.BoardComponents.Any(bc => bc.BoardId == boardId))
            .ToList();
    }
}
