using ServiceA.ContentApi.Entities;

namespace ServiceA.ContentApi.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Contents.Any()) return;

            db.Contents.AddRange(
                new DndGeneratedContent
                {
                    Title = "The Aboleth's Lair",
                    Category = "monster",
                    SrdReference = "aboleth",
                    Prompt = "Using these official D&D stats for the Aboleth (Type: Aberration, Size: Large, Alignment: lawful evil, Hit Points: 135, Challenge Rating: 10): Write a vivid atmospheric description of this monster that a dungeon master could read aloud to players.",
                    GeneratedText = "In the sunken depths beneath the earth, where light dare not venture, the Aboleth holds dominion. An ancient horror of immeasurable intelligence, its vast serpentine body undulates through the black water, trailing three writhing tentacles that leave a shimmering mucus in their wake. Its four eyes — pale, unblinking, and utterly alien — have witnessed the rise and fall of civilizations. To meet its gaze is to feel your memories unravel, your will dissolving like salt in the tide.",
                    CreatedAt = DateTime.UtcNow
                },
                new DndGeneratedContent
                {
                    Title = "The Wrath of Fireball",
                    Category = "spell",
                    SrdReference = "fireball",
                    Prompt = "Using these official D&D details for the spell Fireball (Level: 3, School: Evocation, Casting Time: 1 action, Range: 150 feet): Write a vivid dramatic description of this spell's visual effects and how it feels to cast it.",
                    GeneratedText = "You extend your finger toward the chosen point and speak the final syllable of the incantation. For a heartbeat, nothing happens — then a brilliant streak of flame tears through the air, a burning mote of orange light that seems to hunger for its destination. The explosion is instantaneous and total: a roiling ball of fire erupts outward with a thunderous crack, scorching everything within thirty feet. The heat is felt half a second before the sound reaches you, a wall of searing warmth that makes the air itself shimmer and distort.",
                    CreatedAt = DateTime.UtcNow
                },
            new DndGeneratedContent
            {
                Title = "Mira the Fence",
                Category = "npc",
                SrdReference = null,
                Prompt = "Create a memorable NPC for a D&D campaign. Include their appearance, personality, and a secret they are hiding.",
                GeneratedText = "Mira Ashveil appears to be nothing more than a cheerful pawnbroker, her shop crammed with dusty curiosities and her smile never wavering. She is short and stout, with ink-stained fingers and a pair of spectacles perpetually perched on the end of her nose. She laughs easily, haggles fiercely, and always has a warm word for repeat customers. Her secret: every item she sells has passed through the hands of the Thieves Guild, and the back room of her shop serves as a dead drop for Guild communications. She is not cruel — she simply made a deal twenty years ago and has never found a way out.",
                CreatedAt = DateTime.UtcNow
            },
            new DndGeneratedContent
            {
                Title = "Shadows Over Millhaven",
                Category = "adventure-hook",
                SrdReference = null,
                Prompt = "Write an intriguing adventure hook for a D&D campaign that will draw players into a new quest.",
                GeneratedText = "The farming village of Millhaven has not sent its weekly grain cart to the city in three weeks. When the players investigate, they find the village intact and the villagers seemingly healthy — but wrong. The people speak in clipped, identical sentences. They smile at inappropriate moments. At night, every villager walks to the village well and stares into it in silence for exactly one hour before returning to bed. The well, it turns out, leads down to something very old that has been awake for a very long time, and it has been very lonely.",
                CreatedAt = DateTime.UtcNow
            },
            new DndGeneratedContent
            {
                Title = "The Fall of Nerath",
                Category = "lore",
                SrdReference = null,
                Prompt = "Write a piece of D&D world lore, as if excerpted from an in-world tome or historical record.",
                GeneratedText = "Excerpt from 'The Sundered Empires', Vol. III, compiled by the Archivist Theodan Marsh, 412 AR.\n\nOf the Empire of Nerath, little remains but scattered stonework and the bitter memory of those whose grandparents survived its fall. At its height, Nerath stretched from the Thornwall Mountains to the Amber Sea, a civilization of law, learning, and considerable martial pride. Its fall was not swift. The historians who survived it disagree on causes — some cite the Plague of Whispers, others the withdrawal of divine favour following the Schism of the Three Crowns. What is beyond dispute is the decade of collapse: the armies thinned, the roads went unrepaired, and the monsters crept back into places they had not walked in two hundred years. By the time the last Emperor died without an heir, there was no empire left to inherit.",
                CreatedAt = DateTime.UtcNow
            }
        );

            db.SaveChanges();
        }
    }
}
