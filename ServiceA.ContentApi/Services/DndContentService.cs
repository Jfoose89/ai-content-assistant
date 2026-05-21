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

    public async Task<PagedResult<DndContentResponseDto>> GetAllAsync(DndContentFilterDto filter)
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

        var totalCount = await query.CountAsync();

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToDto(c))
            .ToListAsync();

        return new PagedResult<DndContentResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
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
        if (!string.IsNullOrWhiteSpace(dto.CustomPrompt))
            return dto.CustomPrompt;

        if (!string.IsNullOrWhiteSpace(dto.SrdReference))
        {
            return dto.Category switch
            {
                "monster" => await BuildMonsterPromptAsync(dto.SrdReference),
                "spell" => await BuildSpellPromptAsync(dto.SrdReference),
                _ => BuildGenericPrompt(dto.Category, dto.SrdReference)
            };
        }

        return dto.Category switch
        {
            "monster" =>
                """
            Generate a vivid atmospheric description of a fearsome D&D monster for a Dungeon Master to read aloud.

            Your response must include:
            1. A dramatic introduction of the creature as it appears or approaches
            2. A detailed physical description (body, eyes, movement, aura)
            3. The feeling or dread it inspires in those who witness it

            If you are unsure about any official lore for this creature, describe it atmospherically rather than stating facts.
            Keep the response between 150 and 250 words.
            """,

            "spell" =>
                """
            Generate a vivid dramatic description of a D&D arcane spell being cast, suitable for a Dungeon Master to read aloud.

            Your response must include:
            1. The moment of casting — gestures, words, components
            2. The visual and sensory effects as the spell takes hold
            3. The impact or aftermath as witnesses perceive it

            If you are unsure about the spell's official mechanics, focus on atmosphere rather than stating rules.
            Keep the response between 150 and 250 words.
            """,

            "npc" =>
                """
            Generate a memorable D&D NPC for a Dungeon Master to introduce to their players.

            Your response must include:
            1. Physical appearance — age, build, distinguishing features, clothing
            2. Personality — how they speak, what they want, what they fear
            3. A secret — something they are hiding that a perceptive player might uncover

            Write in present tense as if describing someone the players are meeting right now.
            If you are uncertain about any detail, invent something consistent rather than leaving it vague.
            Keep the response between 150 and 250 words.
            """,

            "adventure-hook" =>
                """
            Generate an intriguing D&D adventure hook that will draw players into a new quest.

            Your response must include:
            1. The inciting incident — what happens or what the players discover
            2. The mystery or threat — what is at stake and why it matters
            3. The hook — a compelling reason for the players to get involved

            Write it as a scene or situation, not a summary. Make it feel urgent and personal.
            If you are unsure how to make it fit a specific setting, keep it setting-neutral.
            Keep the response between 150 and 250 words.
            """,

            "lore" =>
                """
            Generate a piece of D&D world lore written as an excerpt from an ancient in-world tome or historical record.

            Your response must include:
            1. A title for the excerpt (e.g. "From the Annals of the Silver Tower")
            2. The historical or mythological account, written in an archaic, scholarly tone
            3. A closing note hinting at unreliable narration or lost knowledge

            Write entirely in character as the in-world author. Do not break the fictional frame.
            If you are uncertain about lore details, present them as disputed or forgotten history.
            Keep the response between 150 and 250 words.
            """,

            _ => "Write creative D&D content suitable for a tabletop roleplaying game."
        };
    }

    private async Task<string> BuildMonsterPromptAsync(string name)
    {
        var monster = await _srd.GetMonsterAsync(name);
        if (monster is null)
            return $"""
            Generate a vivid atmospheric description of the D&D monster '{name}' for a Dungeon Master to read aloud.

            Your response must include:
            1. A dramatic introduction as the creature appears or approaches
            2. A detailed physical description (body, eyes, movement, presence)
            3. The dread or awe it inspires in those who witness it

            If you are unsure about official lore for '{name}', describe it atmospherically rather than stating facts.
            Keep the response between 150 and 250 words.
            """;

        return $"""
        Using the following official D&D stats for the {monster.Name}:
        - Type: {monster.MonsterTypeName}
        - Size: {monster.Size}
        - Alignment: {monster.Alignment}
        - Hit Points: {monster.HitPoints}
        - Challenge Rating: {monster.ChallengeRating}

        Generate a vivid atmospheric description of this monster for a Dungeon Master to read aloud.

        Your response must include:
        1. A dramatic introduction as the creature appears or approaches
        2. A physical description that reflects its type, size and alignment
        3. The sense of danger appropriate to its Challenge Rating of {monster.ChallengeRating}

        Use the stats to inform the tone and scale of the description — a CR 10 creature should feel far more terrifying than a CR 1.
        If you are unsure about any lore details, describe atmospherically rather than stating facts.
        Keep the response between 150 and 250 words.
        """;
    }

    private async Task<string> BuildSpellPromptAsync(string name)
    {
        var spell = await _srd.GetSpellAsync(name);
        if (spell is null)
            return $"""
            Generate a vivid dramatic description of the D&D spell '{name}' being cast, for a Dungeon Master to read aloud.

            Your response must include:
            1. The moment of casting — gestures, words, components
            2. The visual and sensory effects as the spell takes hold
            3. The impact or aftermath as witnesses perceive it

            If you are unsure about '{name}' official mechanics, focus on atmosphere rather than rules.
            Keep the response between 150 and 250 words.
            """;

        return $"""
        Using the following official D&D details for the spell {spell.Name}:
        - Level: {spell.Level}
        - School: {spell.School}
        - Casting Time: {spell.CastingTime}
        - Range: {spell.Range}

        Generate a vivid dramatic description of this spell being cast, for a Dungeon Master to read aloud.

        Your response must include:
        1. The casting moment — reflecting the casting time of '{spell.CastingTime}'
        2. The visual and sensory effects appropriate to the school of '{spell.School}'
        3. The perceived impact at a range of '{spell.Range}'

        Use the spell details to shape the atmosphere — a touch-range spell feels intimate and dangerous, a long-range spell feels sweeping and powerful.
        If you are unsure about any mechanics, focus on atmosphere rather than rules.
        Keep the response between 150 and 250 words.
        """;
    }

    private static string BuildGenericPrompt(string category, string reference) =>
        $"""
    Write atmospheric D&D content about '{reference}' for the category '{category}'.

    Your response must:
    1. Be written in vivid, evocative prose suitable for reading aloud at a tabletop
    2. Stay faithful to the tone and themes of Dungeons & Dragons 5th Edition
    3. Be between 150 and 250 words

    If you are uncertain about official lore for '{reference}', present it atmospherically rather than stating facts.
    """;

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