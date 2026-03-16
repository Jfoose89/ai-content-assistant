AI Content Assistant
Two cooperating ASP.NET Core 9 Web APIs.
Projects
ProjectPort (HTTPS)ResponsibilityServiceA.ContentApi5001CRUD for AI articles, proxies generation to Service BServiceB.LlmProxy5002Forwards prompts to HuggingFace Inference API
First-time setup
1. User Secrets — Service A
bashcd ServiceA.ContentApi
dotnet user-secrets init
dotnet user-secrets set "ServiceB:ApiKey" "pick-any-secret-string"
2. User Secrets — Service B
bashcd ServiceB.LlmProxy
dotnet user-secrets init
dotnet user-secrets set "ServiceB:ApiKey" "same-secret-string-as-above"
dotnet user-secrets set "HuggingFace:ApiToken" "hf_your_token_from_huggingface"
Get your HuggingFace token at: https://huggingface.co/settings/tokens (Read access is enough)
3. Run both services simultaneously
In Visual Studio: right-click solution ? Properties ? Multiple Startup Projects ? set both to "Start".
Or in two separate terminals:
bash# Terminal 1
cd ServiceA.ContentApi && dotnet run

# Terminal 2
cd ServiceB.LlmProxy && dotnet run
Scalar UI

Service A: https://localhost:5001/scalar
Service B: https://localhost:5002/scalar

Example requests
Create an article (triggers LLM generation)
httpPOST https://localhost:5001/api/articles
Content-Type: application/json

{
  "title": "Introduction to REST APIs",
  "prompt": "Write a short beginner-friendly introduction to REST APIs.",
  "category": "blog"
}
List articles with filtering
httpGET https://localhost:5001/api/articles?category=blog&sort=-createdAt