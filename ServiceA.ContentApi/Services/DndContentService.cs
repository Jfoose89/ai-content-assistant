using Microsoft.EntityFrameworkCore;
using ServiceA.ContentApi.Exceptions;
using ServiceA.ContentApi.Data;
using ServiceA.ContentApi.DTOs;
using ServiceA.ContentApi.Entities;

namespace ServiceA.ContentApi.Services;

public class DndContentService : IDndContentService
{
    private readonly AppDbContext _db;
    private readonly ILlmService _llm;
    private readonly IDndSrdService _srd;

    public DndContentService(AppDbContext db, ILlmService llm, IDndSrdService srd)
    {
        _db = db;
        _llm = llm;
        _srd = srd;
    }

    public async Task<IEnumerable<DndContentResponseDto>> GetAllAsync(DndContentFilterDto filter)
    {
        var query = _db.Contents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(c => c.Category == filter.Category);

        if (filter.StartDate.HasValue)
            query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

        query = filter.Sort switch
        {
            "-createdAt" => query.OrderByDescending(c => c.CreatedAt),
            "createdAt" => query.OrderBy(c => c.CreatedAt),
            "-title" => query.OrderByDescending(c => c.Title),
            "title" => query.OrderBy(c => c.Title),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        return await query.Select(c => ToDto(c)).ToListAsync();
    }

    public async Task<DndContentResponseDto> GetByIdAsync(int id)
    {
        var content = await _db.Contents.FindAsync(id);
        if (content is null)
            throw new NotFoundException($"Content with ID {id} was not found.");
        return ToDto(content);
    }

    public async Task<DndContentResponseDto> CreateAsync(DndContentRequestDto dto)
    {
        // Build the prompt — either from custom input or auto-generated
        var prompt = await BuildPromptAsync(dto);

        // Send prompt to Service B → HuggingFace
        var generated = await _llm.GenerateAsync(prompt);

        var content = new DndGeneratedContent
        {
            Title = dto.Title,
            Category = dto.Category,
            Prompt = prompt,
            GeneratedText = generated.GeneratedText,
            SrdReference = dto.SrdReference,
            CreatedAt = DateTime.UtcNow
        };

        _db.Contents.Add(content);
        await _db.SaveChangesAsync();
        return ToDto(content);
    }

    public async Task<DndContentResponseDto> UpdateAsync(int id, DndContentRequestDto dto)
    {
        var content = await _db.Contents.FindAsync(id);
        if (content is null)
            throw new NotFoundException($"Content with Id {id} was not found.");

        var prompt = await BuildPromptAsync(dto);
        var generated = await _llm.GenerateAsync(prompt);

        content.Title = dto.Title;
        content.Category = dto.Category;
        content.Prompt = prompt;
        content.GeneratedText = generated.GeneratedText;
        content.SrdReference = dto.SrdReference;
        content.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(content);
    }

    public async Task DeleteAsync(int id)
    {
        var content = await _db.Contents.FindAsync(id);
        if (content is null)
            throw new NotFoundException($"Content with ID {id} was not found.");

        _db.Contents.Remove(content);
        await _db.SaveChangesAsync();
    }

    // ── Prompt builder ────────────────────────────────────────────────────────

    private async Task<string> BuildPromptAsync(DndContentRequestDto dto)
    {
        // If the user provided a custom prompt, use it directly
        if (!string.IsNullOrWhiteSpace(dto.CustomPrompt))
            return dto.CustomPrompt;

        // If an SRD reference was provided, try to enrich the prompt
        if (!string.IsNullOrWhiteSpace(dto.SrdReference))
        {
            return dto.Category switch
            {
                "monster" => await BuildMonsterPromptAsync(dto.SrdReference),
                "spell" => await BuildSpellPromptAsync(dto.SrdReference),
                _ => BuildGenericPrompt(dto.Category, dto.SrdReference)
            };
        }

        // Fallback: generate a prompt from category alone
        return dto.Category switch
        {
            "monster" => "Describe a fearsome monster for a D&D encounter. Write a vivid atmospheric description a dungeon master could read aloud.",
            "spell" => "Describe a dramatic arcane spell for D&D. Write a vivid description of its visual effects and how it feels to cast it.",
            "npc" => "Create a memorable NPC for a D&D campaign. Include their appearance, personality, and a secret they are hiding.",
            "adventure-hook" => "Write an intriguing adventure hook for a D&D campaign that will draw players into a new quest.",
            "lore" => "Write a piece of D&D world lore, as if excerpted from an in-world tome or historical record.",
            _ => "Write creative D&D content suitable for a tabletop roleplaying game."
        };
    }

    private async Task<string> BuildMonsterPromptAsync(string name)
    {
        var monster = await _srd.GetMonsterAsync(name);
        if (monster is null)
            return $"Describe a fearsome {name} for a D&D encounter. Write a vivid atmospheric description a dungeon master could read aloud.";

        return $"Using these official D&D stats for the {monster.Name} " +
               $"(Type: {monster.MonsterTypeName}, Size: {monster.Size}, " +
               $"Alignment: {monster.Alignment}, Hit Points: {monster.HitPoints}, " +
               $"Challenge Rating: {monster.ChallengeRating}): " +
               $"Write a vivid atmospheric description of this monster that a dungeon master could read aloud to players.";
    }

    private async Task<string> BuildSpellPromptAsync(string name)
    {
        var spell = await _srd.GetSpellAsync(name);
        if (spell is null)
            return $"Describe the D&D spell '{name}'. Write a vivid description of its visual effects and how it feels to cast it.";

        return $"Using these official D&D details for the spell {spell.Name} " +
               $"(Level: {spell.Level}, School: {spell.School}, " +
               $"Casting Time: {spell.CastingTime}, Range: {spell.Range}): " +
               $"Write a vivid dramatic description of this spell's visual effects and how it feels to cast it.";
    }

    private static string BuildGenericPrompt(string category, string reference) =>
        $"Write D&D content about '{reference}' for the category '{category}'. " +
        $"Make it atmospheric and suitable for a tabletop roleplaying game.";

    // ── Mapper ────────────────────────────────────────────────────────────────

    private static DndContentResponseDto ToDto(DndGeneratedContent c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Prompt = c.Prompt,
        GeneratedText = c.GeneratedText,
        Category = c.Category,
        SrdReference = c.SrdReference,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}