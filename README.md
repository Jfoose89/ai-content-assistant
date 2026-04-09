AI Content Assistant — D&D Content Generator
A distributed microservices system built with ASP.NET Core (.NET 9), consisting of two cooperating Web APIs that generate atmospheric D&D content using AI. The system integrates with a local D&D SRD Rules Database API to enrich prompts with real stat block data before sending them to HuggingFace for generation.

How It Works

A client sends a POST request to Service A with a D&D category and optional SRD reference
Service A calls the dnd-srd API to fetch real stat block data (e.g. monster HP, CR, type)
Service A builds an enriched prompt using the real SRD data
The enriched prompt is forwarded to Service B via a typed HTTP client
Service B forwards the prompt to HuggingFace and returns the generated text
Service A saves the generated content to its in-memory database and returns the result


Services
ServiceA.ContentApi — runs on port 5001
CRUD for D&D content, SRD enrichment, proxies generation requests to Service B
ServiceB.LlmProxy — runs on port 5002
Validates API key, forwards prompts to HuggingFace
Service A depends on two external services:

dnd-srd API (https://localhost:7120) — provides real SRD stat blocks to enrich prompts
Service B (http://localhost:5003) — proxies generation requests to HuggingFace


Tech Stack

-Framework: ASP.NET Core Web API (.NET 9)
-Database: EF Core In-Memory
-Documentation: Scalar UI
-HTTP Clients: IHttpClientFactory with Typed Clients
-External LLM: HuggingFace Inference API (router.huggingface.co)
-SRD Data: Local dnd-srd API (https://localhost:7120)
-Security: API key validation between services via X-Api-Key header
-Error Handling: Custom Exception Middleware with RFC 7807 ProblemDetails


Supported D&D Content Categories

-monster — Atmospheric monster descriptions for DMs. Fetches real stat block from dnd-srd if an SRD reference is provided.
-spell — Vivid spell effect descriptions. Fetches real spell details from dnd-srd if an SRD reference is provided.
-npc — Memorable NPC with appearance, personality and a secret.
-adventure-hook — Intriguing quest hooks to draw players into a new quest.
-lore — In-world historical records and lore entries written as if from an ancient tome.


Prerequisites

-.NET 9 SDK
-Visual Studio 2022
-A free HuggingFace account with an access token
-The dnd-srd API running locally on port 7120


First-Time Setup
1. HuggingFace Token
Create a free account at huggingface.co, then go to Settings → Access Tokens → Create new token. Select Fine-grained and enable "Make calls to Inference Providers". Copy the token.
2. User Secrets — Service A
bashcd ServiceA.ContentApi
dotnet user-secrets init
dotnet user-secrets set "ServiceB:ApiKey" "pick-any-secret-string"
3. User Secrets — Service B
bashcd ServiceB.LlmProxy
dotnet user-secrets init
dotnet user-secrets set "ServiceB:ApiKey" "same-secret-string-as-above"
dotnet user-secrets set "HuggingFace:ApiToken" "hf_your_token_here"
The ServiceB:ApiKey is a shared secret you invent — it must match in both services. Service B uses it to verify that requests are coming from Service A.
4. Run all three services
Start the dnd-srd API first, then start this solution.
In Visual Studio: right-click the solution → Properties → Multiple Startup Projects → set both ServiceA.ContentApi and ServiceB.LlmProxy to Start.
Or in separate terminals:
bash# Terminal 1 — dnd-srd (separate solution)
cd path/to/dnd-srd && dotnet run

# Terminal 2 — Service A
-cd ServiceA.ContentApi && dotnet run

# Terminal 3 — Service B
-cd ServiceB.LlmProxy && dotnet run

Scalar UI

-Service A — D&D Content API: https://localhost:5001/scalar
-Service B — LLM Proxy API: https://localhost:5002/scalar


Endpoints
-Service A — D&D Content API

-GET /api/DndContent — Returns paginated content with optional filtering and sorting
-GET /api/DndContent/{id} — Returns a single content entry by ID
-POST /api/DndContent — Creates and generates new D&D content
-PUT /api/DndContent/{id} — Updates and regenerates an existing entry
-DELETE /api/DndContent/{id} — Deletes a content entry

Query parameters for GET /api/DndContent:

-category — Filter by category (monster, spell, npc, adventure-hook, lore)
-startDate — Filter entries created after this date
-endDate — Filter entries created before this date
-sort — Sort order: createdAt, -createdAt, title, -title
-page - Page number (1-based, default: 1)
-pageSize - Number of items per page (default: 10, max: 100)

Service B — LLM Proxy API

POST /api/Llm/generate — Generates text via HuggingFace (requires X-Api-Key header)


Example Requests
Generate a monster description with SRD enrichment
jsonPOST https://localhost:5001/api/DndContent
Content-Type: application/json

{
  "title": "Aboleth Encounter",
  "category": "monster",
  "srdReference": "aboleth"
}
Service A will fetch the Aboleth's real stat block from dnd-srd and build an enriched prompt before calling HuggingFace.
Generate a spell description with SRD enrichment
jsonPOST https://localhost:5001/api/DndContent
Content-Type: application/json

{
  "title": "The Wrath of Fireball",
  "category": "spell",
  "srdReference": "fireball"
}
Generate an NPC
jsonPOST https://localhost:5001/api/DndContent
Content-Type: application/json

{
  "title": "The Mysterious Innkeeper",
  "category": "npc"
}
Generate content with a custom prompt
jsonPOST https://localhost:5001/api/DndContent
Content-Type: application/json

{
  "title": "The Ancient Dragon",
  "category": "monster",
  "customPrompt": "Describe an ancient red dragon awakening from a century of slumber, its scales glowing like embers, as it surveys its mountain domain for the first time."
}
Filter and sort saved content
GET https://localhost:5001/api/DndContent?category=monster&sort=-createdAt

Error Responses
All errors follow the RFC 7807 ProblemDetails format:
json{
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Content with ID 999 was not found.",
  "instance": "/api/DndContent/999"
}
Status codes:

-200 — OK
-201 — Created
-204 — No Content
-400 — Bad Request (validation error)
-401 — Unauthorized (missing or invalid API key)
-404 — Not Found (resource does not exist)
-500 — Internal Server Error


Project Structure
-ServiceA.ContentApi
Controllers

-DndContentController.cs — CRUD endpoints for D&D content

Data

-AppDbContext.cs — EF Core In-Memory database context

Entities

-DndGeneratedContent.cs — Database entity for generated content

Exceptions

-AppExceptions.cs — Custom exception classes (NotFoundException, ValidationException)

Filters

-ValidationFilter.cs — Automatically validates ModelState before controller actions

Services

-IDndContentService.cs / DndContentService.cs — Business logic and prompt building
-DndSrdService.cs — Typed HTTP client for fetching SRD data from dnd-srd API
-ILlmService.cs / LlmService.cs — Typed HTTP client for forwarding prompts to Service B

Root

-ExceptionMiddleware.cs — Catches all unhandled exceptions and returns RFC 7807 ProblemDetails
-Program.cs — Service registration and middleware pipeline


ServiceB.LlmProxy
Controllers

-LlmController.cs — Generation endpoint with API key validation

Services

-HuggingFaceService.cs — Typed HTTP client for the HuggingFace Inference API

Root

-Program.cs — Service registration and middleware pipeline
