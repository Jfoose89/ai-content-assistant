using Microsoft.AspNetCore.Mvc;
using ServiceA.ContentApi.DTOs;
using ServiceA.ContentApi.Filters;
using ServiceA.ContentApi.Services;

namespace ServiceA.ContentApi.Controllers;

/// <summary>Manages AI-generated D&D content (CRUD + generation via Service B).</summary>
[ApiController]
[Route("api/[controller]")]
[ValidationFilter]
public class DndContentController : ControllerBase
{
    private readonly IDndContentService _contentService;

    public DndContentController(IDndContentService contentService)
    {
        _contentService = contentService;
    }

    /// <summary>Returns all D&D content entries, with optional filtering and sorting.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DndContentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] DndContentFilterDto filter)
    {
        var contents = await _contentService.GetAllAsync(filter);
        return Ok(contents);
    }

    /// <summary>Returns a single content entry by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DndContentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var content = await _contentService.GetByIdAsync(id);
        return Ok(content);
    }

    /// <summary>
    /// Creates a new D&D content entry. Optionally enriches the prompt 
    /// with real SRD data before generating text via Service B.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DndContentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DndContentRequestDto dto)
    {
        var created = await _contentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing content entry and regenerates its text.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DndContentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] DndContentRequestDto dto)
    {
        var updated = await _contentService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Deletes a content entry by ID.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _contentService.DeleteAsync(id);
        return NoContent();
    }
}