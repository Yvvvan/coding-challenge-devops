using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Api.Services;

public interface IComponentService
{
    Task<List<ComponentDto>> GetAllAsync();
    Task<ComponentDto?> GetByIdAsync(int id);
    Task<List<ComponentDto>> SearchAsync(string? term);
    Task<ComponentPageDto> GetPageAsync(string? term, int page, int pageSize);
    Task<ComponentDto> CreateAsync(ComponentCreateDto dto);
    Task<ComponentDto?> UpdateAsync(int id, ComponentUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public class ComponentService : IComponentService
{
    private readonly IComponentManager _componentManager;
    private readonly ILogger<ComponentService> _logger;

    public ComponentService(IComponentManager componentManager, ILogger<ComponentService> logger)
    {
        _componentManager = componentManager;
        _logger = logger;
    }

    public async Task<List<ComponentDto>> GetAllAsync()
    {
        var components = await _componentManager.GetAllAsync();
        return components.Select(c => ToDto(c, new List<BoardSummaryDto>())).ToList();
    }

    public async Task<ComponentDto?> GetByIdAsync(int id)
    {
        var component = await _componentManager.GetByIdAsync(id);
        return component is null ? null : ToDto(component, MapBoards(component));
    }

    public async Task<List<ComponentDto>> SearchAsync(string? term)
    {
        var components = await _componentManager.SearchAsync(term);
        return components.Select(c => ToDto(c, new List<BoardSummaryDto>())).ToList();
    }

    public async Task<ComponentPageDto> GetPageAsync(string? term, int page, int pageSize)
    {
        var (items, totalCount) = await _componentManager.GetPageAsync(term, page, pageSize);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ComponentPageDto(
            items.Select(c => ToDto(c, new List<BoardSummaryDto>())).ToList(),
            totalCount,
            page,
            pageSize,
            totalPages);
    }

    public async Task<ComponentDto> CreateAsync(ComponentCreateDto dto)
    {
        var component = new Component
        {
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity
        };

        var created = await _componentManager.AddAsync(component);
        _logger.LogInformation("Created component {ComponentId} ({ComponentName})", created.Id, created.Name);
        return ToDto(created, new List<BoardSummaryDto>());
    }

    public async Task<ComponentDto?> UpdateAsync(int id, ComponentUpdateDto dto)
    {
        var component = new Component
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity
        };

        var updated = await _componentManager.UpdateAsync(component);
        if (updated is null)
        {
            return null;
        }

        _logger.LogInformation("Updated component {ComponentId}", id);
        var reloaded = await _componentManager.GetByIdAsync(id);
        return ToDto(reloaded!, MapBoards(reloaded!));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _componentManager.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Deleted component {ComponentId}", id);
        }

        return deleted;
    }

    private static List<BoardSummaryDto> MapBoards(Component component)
    {
        return component.BoardComponents
            .Select(bc => new BoardSummaryDto(bc.Board.Id, bc.Board.Name, bc.Board.Length, bc.Board.Width))
            .ToList();
    }

    private static ComponentDto ToDto(Component component, List<BoardSummaryDto> boards)
    {
        return new ComponentDto(component.Id, component.Name, component.Description, component.Quantity, boards);
    }
}
