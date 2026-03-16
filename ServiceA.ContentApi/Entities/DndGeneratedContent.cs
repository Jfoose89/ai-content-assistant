namespace ServiceA.ContentApi.Entities;

public class DndGeneratedContent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;

    /// <summary>
    /// Category of D&D content, monster, spell, npc, adventure-hook, lore
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional: the name of the SRD entity used to enrich the prompt
    /// e.g. "aboleth", "fireball", "mordenkainen's sword"
    /// </summary>
    public string? SrdReference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}