using System.ComponentModel.DataAnnotations;

namespace ServiceA.ContentApi.DTOs;

/// <summary>Request body for generating and saving D&D content.</summary>
public class DndContentRequestDto
{
    /// <summary>A title for this piece of generated content.</summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Category of D&D content: monster, spell, npc, adventure-hook, lore</summary>
    [Required(ErrorMessage = "Category is required.")]
    [RegularExpression("^(monster|spell|npc|adventure-hook|lore)$",
    ErrorMessage = "Invalid category. Must be one of: monster, spell, npc, adventure-hook, lore")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional: name of a real SRD entity to enrich the prompt with.
    /// e.g. "aboleth" for category monster, "fireball" for category spell.
    /// If provided, Service A will look up this entity in the dnd-srd API.
    /// </summary>
    public string? SrdReference { get; set; }

    /// <summary>
    /// Optional: a custom prompt override. If not provided, one will be
    /// auto-generated based on the category and SrdReference.
    /// </summary>
    public string? CustomPrompt { get; set; }
}

/// <summary>Response body returned to clients.</summary>
public class DndContentResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? SrdReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Query parameters for filtering the content list.</summary>
public class DndContentFilterDto
{
    public string? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    /// <summary>Sort field. Prefix with '-' for descending, e.g. "-createdAt".</summary>
    public string? Sort { get; set; }
    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int Page { get; set; } = 1;
    /// <summary>Number of items per page. Defaults to 10.</summary>
    public int PageSize { get; set; } = 10;
}

// Add this at the bottom of the file, after GenerateResponseDto:
/// <summary>Generic paginated result wrapper.</summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
/// <summary>Request body sent from Service A to Service B.</summary>
public class GenerateRequestDto
{
    [Required]
    public string Prompt { get; set; } = string.Empty;
}

/// <summary>Response received from Service B.</summary>
public class GenerateResponseDto
{
    public string GeneratedText { get; set; } = string.Empty;
}