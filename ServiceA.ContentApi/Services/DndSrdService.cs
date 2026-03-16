using System.Text.Json.Serialization;
using ServiceA.ContentApi.DTOs;

namespace ServiceA.ContentApi.Services;

public class SrdMonster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Alignment { get; set; }
    public int? HitPoints { get; set; }
    public double? ChallengeRating { get; set; }
    public string? MonsterTypeName { get; set; }
}

public class SrdSpell
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Level { get; set; }
    public string? School { get; set; }
    public string? CastingTime { get; set; }
    public string? Range { get; set; }
    public string? Duration { get; set; }
    public string? Description { get; set; }
    public string? ClassName { get; set; }
}

public class SrdPagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

public interface IDndSrdService
{
    Task<SrdMonster?> GetMonsterAsync(string name);
    Task<SrdSpell?> GetSpellAsync(string name);
}

/// <summary>
/// Typed HTTP client that fetches SRD data from the local dnd-srd API
/// to enrich prompts before sending them to HuggingFace.
/// </summary>
public class DndSrdService : IDndSrdService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DndSrdService> _logger;

    public DndSrdService(HttpClient httpClient, ILogger<DndSrdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SrdMonster?> GetMonsterAsync(string name)
    {
        try
        {
            // Step 1: search by name to get the ID
            var search = await _httpClient
                .GetFromJsonAsync<SrdPagedResult<SrdMonster>>(
                    $"/api/monsters?name={Uri.EscapeDataString(name)}&pageSize=1");

            var found = search?.Data?.FirstOrDefault();
            if (found is null) return null;

            // Step 2: fetch full details by ID
            var monster = await _httpClient
                .GetFromJsonAsync<SrdMonster>($"/api/monsters/{found.Id}");

            return monster;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not fetch monster '{Name}' from dnd-srd API: {Message}",
                name, ex.Message);
            return null;
        }
    }

    public async Task<SrdSpell?> GetSpellAsync(string name)
    {
        try
        {
            // Step 1: search by name to get the ID
            var search = await _httpClient
                .GetFromJsonAsync<SrdPagedResult<SrdSpell>>(
                    $"/api/spells?name={Uri.EscapeDataString(name)}&pageSize=1");

            var found = search?.Data?.FirstOrDefault();
            if (found is null) return null;

            // Step 2: fetch full details by ID
            var spell = await _httpClient
                .GetFromJsonAsync<SrdSpell>($"/api/spells/{found.Id}");

            return spell;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not fetch spell '{Name}' from dnd-srd API: {Message}",
                name, ex.Message);
            return null;
        }
    }
}