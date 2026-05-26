# AI Output Evaluation — D&D Content Assistant 2.0

## What Counts as Good Output

Before evaluating results, these are the criteria used to judge quality:

- **Relevance** — the response directly addresses the requested category and SRD reference
- **Atmosphere** — the writing is vivid and suitable for reading aloud at a D&D table
- **Structure** — the response follows the requested format (heading, body, DM note)
- **Accuracy** — no invented mechanics or stats that contradict official 5e rules
- **Length** — between 150 and 250 words as instructed
- **Uncertainty handling** — when unsure, the model describes atmospherically rather than fabricating facts

---

## Test 1 — Monster with SRD Reference

**Request:**
- Title: The Aboleth Rises
- Category: monster
- SRD Reference: aboleth

**What the prompt included:**
The system fetched real SRD stats for the aboleth (Type, Size, Alignment, Hit Points, Challenge Rating)
and instructed the model to use them to shape the tone and scale of the description.

**Generated output summary:**
The model produced a structured response with a heading, three clear paragraphs covering
introduction, physical description and dread, and a closing DM note suggesting a Wisdom
saving throw mechanic.

**What was good:**
- Followed the requested format precisely — heading, atmospheric prose, italic DM note
- The language was vivid and genuinely suitable for reading aloud
- The DM note was practical and mechanically reasonable
- Correctly conveyed the aboleth as an alien, deeply unsettling entity consistent with its lore

**What was bad:**
- The DM note invented specific mechanics ("roll a Wisdom saving throw... debilitating fear")
that are not part of the official aboleth stat block — this is a hallucination of game rules
- The phrase "some claim to have beheld the gods themselves" is dramatic but vague,
adding little concrete atmosphere
- Word count was slightly above the 250 word target

**What was changed after this test:**
The system prompt in ServiceB already includes the rule "never invent game mechanics or
spell stats that contradict official 5e rules" and "if uncertain, say so rather than guessing."
However the model still hallucinated mechanics in the DM note. This suggests the instruction
needs to be more explicit — future improvement would add "do not invent saving throws,
damage values or conditions not present in the provided stats."

---

## Test 2 — Spell with SRD Reference

**Request:**
- Title: Fireball Unleashed
- Category: spell
- SRD Reference: fireball

**What the prompt included:**
The system fetched real SRD details for Fireball (Level 3, Evocation, 1 action casting time,
150 feet range) and instructed the model to use each detail to shape the atmosphere.

**Generated output summary:**
The model produced a heading, three paragraphs covering the casting moment, visual effects
and impact, and a closing DM note. It correctly used the 150-foot range as a dramatic device.

**What was good:**
- The casting moment was well-paced and reflected the "1 action" casting time naturally
- The Evocation school was reflected correctly — raw, explosive, elemental force
- The 150-foot range was used effectively to convey the sweeping, powerful nature of the spell
- The sensory details (heat, scent of charred earth) were vivid and atmospheric

**What was bad:**
- The DM note invented a blast radius formula ("20-foot radius + 4 feet per caster level")
that does not match the official Fireball description — a clear hallucination
- The title generated was "Inferno Unleashed" rather than "Fireball Unleashed" — the model
ignored the title field entirely and invented its own, which is inconsistent
- The DM note broke the atmospheric tone by becoming overly mechanical and tactical

**What was changed after this test:**
The hallucinated blast radius formula confirms the model struggles to distinguish between
"describe the atmosphere" and "state the rules." The DM note instruction in the system prompt
could be tightened to: "End with a short DM note focused on narrative or roleplay guidance
only — never state damage values, ranges, or mechanical formulas."

---

## Test 3 — NPC with No SRD Reference

**Request:**
- Title: The Mysterious Innkeeper
- Category: npc
- SRD Reference: none

**What the prompt included:**
No SRD data was available — the model relied entirely on the structured prompt instructions
(appearance, personality, secret) and the system prompt rules.

**Generated output summary:**
The model produced a character named "The Blackwood Bard" — notably not an innkeeper —
with a heading, three paragraphs covering appearance, personality and secret, and a closing DM note.

**What was good:**
- The three required sections (appearance, personality, secret) were all present and well written
- The secret was genuinely interesting and usable at a real table
- The writing was in present tense as instructed
- The DM note was narrative-focused rather than mechanical — the best DM note of the three tests

**What was bad:**
- The model ignored the title "The Mysterious Innkeeper" and generated a bard instead —
this is a significant relevance failure. The title was not passed as part of the user prompt,
only as metadata, so the model had no way to use it
- No physical description of an innkeeper environment or context was provided,
leading the model to invent a completely different character type

**What was changed after this test:**
The title field should be included in the user prompt itself so the model can use it as context.
For example: "Generate a memorable D&D NPC named '{title}' for a Dungeon Master to introduce."
This was identified as a prompt improvement to implement — passing the title into the prompt
directly so the model generates a character consistent with what was requested.

---

## Test 4 — Lore with No SRD Reference

**Request:**
- Title: Dragons
- Category: lore
- SRD Reference: none

**What the prompt included:**
No SRD data was available — the model relied entirely on the structured prompt instructions
and the system prompt rules. The lore category instructs the model to write as an excerpt
from an in-world historical tome or ancient record.

**Generated output summary:**
The model produced a heading ("From the Chroniclers of Eldrador"), two substantial paragraphs
covering the mythological account of the Primordials and the Great Devouring, and a closing
DM note. Latency was 736ms as measured by the new stopwatch logging added to HuggingFaceService.

**What was good:**
- The in-world tome framing was followed precisely — archaic, scholarly tone throughout
- The closing note hinting at unreliable narration ("the ink fading into obscurity") was
  atmospheric and fit the lore category well
- The DM note was narrative-focused rather than mechanical, consistent with the lore format
- Response was within the 150–250 word target

**What was bad:**
- The connection to dragons specifically was thin — the Primordials account felt more like
  general cosmological lore than dragon-specific history
- "The Great Devouring" is invented lore with no basis in official 5e canon, though this is
  expected and acceptable for the lore category

**What was changed after this test:**
No prompt changes made. The lore category performs well for atmospheric world-building.
For more focused results, the title should be passed directly into the user prompt
(e.g. "Write a lore entry about: {title}") — the same improvement identified in Test 3.

## Prompt Techniques Used

The following concrete prompt engineering techniques were applied in this project:

- **Numbered requirements** — every prompt lists exactly what the response must include
(e.g. "1. Physical appearance, 2. Personality, 3. A secret") rather than leaving it open-ended
- **Uncertainty handling** — every prompt includes an explicit instruction:
"If you are uncertain, describe atmospherically rather than stating facts"
- **Word count constraint** — every prompt specifies "Keep the response between 150 and 250 words"
to prevent excessively long or short responses
- **Role and tone in system prompt** — the system prompt establishes the model as a
D&D creative assistant with explicit rules about what it may and may not do
- **Stat-informed prompting** — for monsters and spells, real SRD data is injected into
the prompt with instructions on how to use each value (e.g. "use the CR to inform the
sense of danger")

---

## Limitations

- **Hallucination of game mechanics** — the model consistently invents saving throws,
damage formulas and mechanical rules that are not present in the source data.
This is the most significant limitation for a D&D use case where accuracy matters.
- **Prompt sensitivity** — small changes in wording produce noticeably different results.
The NPC test showed that omitting the title from the user prompt caused the model to
ignore it entirely.
- **Inconsistent instruction following** — the model sometimes ignores constraints such
as word count or title, particularly when the instruction appears only in the system prompt
rather than being reinforced in the user prompt.
- **Bias toward dramatic language** — the model tends toward overwrought prose
("a sound like the crackling of a thousand blazing thorns") which may not suit all DM styles.
- **No memory between requests** — each generation is independent. The model cannot
build on previous outputs or maintain consistency across a campaign session.

---

## Conclusion

The AI service is well suited for generating atmospheric, readable prose that a Dungeon Master
can use as a starting point or inspiration. It reliably follows structural formatting instructions
and produces content that is genuinely usable at a D&D table.

It is not suited for generating mechanically accurate rules content, stat blocks, or anything
where factual correctness is critical. The model will hallucinate game mechanics confidently
and without any signal of uncertainty, even when the system prompt instructs it to flag
uncertainty explicitly.

The most effective use of this service is as a creative writing tool rather than a rules reference —
give it atmosphere and structure to work within, and validate any mechanical claims it makes
against official sources before using them at the table.