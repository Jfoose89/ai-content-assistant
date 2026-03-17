AI Content Assistant — D&D Content Generator
A distributed microservices system built with ASP.NET Core (.NET 9), consisting of two cooperating Web APIs that generate atmospheric D&D content using AI. The system integrates with a local D&D SRD Rules Database API to enrich prompts with real stat block data before sending them to HuggingFace for generation.

System Architecture
Client (Scalar / HTTP)
        │
        ▼
┌─────────────────────────┐
│   Service A – Port 5001 │  D&D Content API
│  ┌──────────────────┐   │
│  │  DndContent      │   │
│  │  Controller      │   │
│  └────────┬─────────┘   │
│           │              │
│  ┌────────▼─────────┐   │         ┌─────────────────────────┐
│  │  DndSrdService   │────────────▶│  dnd-srd API – Port 7120│
│  └──────────────────┘   │         │  (Real SRD stat blocks) │
│  ┌──────────────────┐   │         └─────────────────────────┘
│  │  LlmService      │────────────▶┌─────────────────────────┐
│  └──────────────────┘   │         │  Service B – Port 5002  │  LLM Proxy
│  ┌──────────────────┐   │         │  ┌───────────────────┐  │
│  │  EF Core DB      │   │         │  │  HuggingFaceService│  │
│  │  (In-Memory)     │   │         │  └────────┬──────────┘  │
│  └──────────────────┘   │         └───────────┼─────────────┘
└─────────────────────────┘                     │
                                                 ▼
                                     HuggingFace Inference API
                                     (router.huggingface.co)
How it works

A client sends a POST request to Service A with a D&D category and optional SRD reference
Service A calls the dnd-srd API to fetch real stat block data (e.g. monster HP, CR, type)
Service A builds an enriched prompt using the real SRD data
The enriched prompt is forwarded to Service B via a typed HTTP client
Service B forwards the prompt to HuggingFace and returns the generated text
Service A saves the generated content to its in-memory database and returns the result


Projects
ProjectPort (HTTPS)ResponsibilityServiceA.ContentApi5001CRUD for D&D content, SRD enrichment, proxies generation to Service BServiceB.LlmProxy5002Validates API key, forwards prompts to HuggingFace

Tech Stack

Framework: ASP.NET Core Web API (.NET 9)
Database: EF Core In-Memory
Documentation: Scalar UI
HTTP Clients: IHttpClientFactory with Typed Clients
External LLM: HuggingFace Inference API (router.huggingface.co)
SRD Data: Local dnd-srd API (https://localhost:7120)
Security: API key validation between services via X-Api-Key header
Error Handling: Custom Exception Middleware with RFC 7807 ProblemDetails


Supported D&D Content Categories
CategoryDescriptionSRD EnrichmentmonsterAtmospheric monster descriptions for DMsYes — fetches real stat blockspellVivid spell effect descriptionsYes — fetches real spell detailsnpcMemorable NPC with appearance, personality and secretNoadventure-hookIntriguing quest hooks to draw players inNoloreIn-world historical records and lore entriesNo

Prerequisites

.NET 9 SDK
Visual Studio 2022
A free HuggingFace account with an access token
The dnd-srd API running locally on port 7120


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
cd ServiceA.ContentApi && dotnet run

# Terminal 3 — Service B
cd ServiceB.LlmProxy && dotnet run

Scalar UI
ServiceURLService A — D&D Content APIhttps://localhost:5001/scalarService B — LLM Proxy APIhttps://localhost:5002/scalar

Endpoints
Service A — D&D Content API
MethodEndpointDescriptionGET/api/DndContentReturns all content with optional filteringGET/api/DndContent/{id}Returns a single content entry by IDPOST/api/DndContentCreates and generates new D&D contentPUT/api/DndContent/{id}Updates and regenerates an existing entryDELETE/api/DndContent/{id}Deletes a content entry
Query Parameters for GET /api/DndContent:
ParameterTypeDescriptioncategorystringFilter by category (monster, spell, npc, etc.)startDatedatetimeFilter entries created after this dateendDatedatetimeFilter entries created before this datesortstringSort order: createdAt, -createdAt, title, -title
Service B — LLM Proxy API
MethodEndpointDescriptionAuth RequiredPOST/api/Llm/generateGenerates text via HuggingFaceYes — X-Api-Key

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
Status CodeMeaning200OK201Created204No Content400Bad Request — validation error401Unauthorized — missing or invalid API key404Not Found — resource does not exist500Internal Server Error

Project Structure
AiContentAssistant/
├── ServiceA.ContentApi/
│   ├── Controllers/
│   │   └── DndContentController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── DTOs/
│   │   └── ArticleDtos.cs
│   ├── Entities/
│   │   └── DndGeneratedContent.cs
│   ├── Exceptions/
│   │   └── AppExceptions.cs
│   ├── Filters/
│   │   └── ValidationFilter.cs
│   ├── Services/
│   │   ├── IDndContentService.cs
│   │   ├── DndContentService.cs
│   │   ├── IDndSrdService.cs (in DndSrdService.cs)
│   │   ├── DndSrdService.cs
│   │   ├── ILlmService.cs
│   │   └── LlmService.cs
│   ├── ExceptionMiddleware.cs
│   ├── appsettings.json
│   └── Program.cs
│
└── ServiceB.LlmProxy/
    ├── Controllers/
    │   └── LlmController.cs
    ├── DTOs/
    │   └── LlmDtos.cs
    ├── Services/
    │   └── HuggingFaceService.cs
    ├── appsettings.json
    └── Program.cs
