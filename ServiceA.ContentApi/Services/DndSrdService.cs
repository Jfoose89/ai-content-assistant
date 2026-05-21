using System.Text.Json;

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
    public string? ClassName { get; set; }
}

public class SrdData
{
    public List<SrdMonster> Monsters { get; set; } = new();
    public List<SrdSpell> Spells { get; set; } = new();
}

public interface IDndSrdService
{
    Task<SrdMonster?> GetMonsterAsync(string name);
    Task<SrdSpell?> GetSpellAsync(string name);
}

/// <summary>
/// Reads SRD monster and spell data from a bundled JSON file.
/// No external service dependency required.
/// </summary>
public class DndSrdService : IDndSrdService
{
    private readonly SrdData _data;
    private readonly ILogger<DndSrdService> _logger;

    public DndSrdService(ILogger<DndSrdService> logger)
    {
        _logger = logger;

        var path = Path.Combine(AppContext.BaseDirectory, "Data", "srd-data.json");

        if (!File.Exists(path))
        {
            _logger.LogWarning("srd-data.json not found at {Path}. SRD enrichment will be unavailable.", path);
            _data = new SrdData();
            return;
        }

        var json = File.ReadAllText(path);
        _data = JsonSerializer.Deserialize<SrdData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new SrdData();

        _logger.LogInformation("SRD data loaded: {Monsters} monsters, {Spells} spells.",
            _data.Monsters.Count, _data.Spells.Count);
    }

    public Task<SrdMonster?> GetMonsterAsync(string name)
    {
        var monster = _data.Monsters.FirstOrDefault(m =>
        m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (monster is null)
            _logger.LogWarning("Monster '{Name}' not found in local SRD data.", name);

        return Task.FromResult(monster);
    }

    public Task<SrdSpell?> GetSpellAsync(string name)
    {
        var spell = _data.Spells.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (spell is null)
            _logger.LogWarning("Spell '{Name}' not found in local SRD data.", name);

        return Task.FromResult(spell);
    }
}