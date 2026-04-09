using ServiceA.ContentApi.DTOs;

namespace ServiceA.ContentApi.Services;

public interface IDndContentService
{
    Task<PagedResult<DndContentResponseDto>> GetAllAsync(DndContentFilterDto filter);
    Task<DndContentResponseDto> GetByIdAsync(int id);
    Task<DndContentResponseDto> CreateAsync(DndContentRequestDto dto);
    Task<DndContentResponseDto> UpdateAsync(int id, DndContentRequestDto dto);
    Task DeleteAsync(int id);
}