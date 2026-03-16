using Scalar.AspNetCore;
using ServiceB.LlmProxy.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI / Scalar
builder.Services.AddOpenApi();

// Typed HTTP Client → HuggingFace
builder.Services.AddHttpClient<IHuggingFaceService, HuggingFaceService>(client =>
{
    var baseUrl = builder.Configuration["HuggingFace:BaseUrl"]
        ?? "https://api-inference.huggingface.co";
    var token = builder.Configuration["HuggingFace:ApiToken"]
        ?? throw new InvalidOperationException("HuggingFace:ApiToken is not configured. Use dotnet user-secrets.");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
});

// Ports
builder.WebHost.UseUrls("https://localhost:5002", "http://localhost:5003");

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler("/error");

// NOTE: No UseHttpsRedirection here — Service A calls us over plain HTTP internally

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Scalar UI
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Service B – LLM Proxy API";
});

app.Run();