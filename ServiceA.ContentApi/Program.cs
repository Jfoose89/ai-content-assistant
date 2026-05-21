using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ServiceA.ContentApi;
using ServiceA.ContentApi.Data;
using ServiceA.ContentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── OpenAPI / Scalar ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Database (EF Core In-Memory) ─────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("DndContentDb"));

// ── Typed HTTP Client → Service B ────────────────────────────────────────────
builder.Services.AddHttpClient<ILlmService, LlmService>(client =>
{
    var baseUrl = builder.Configuration["ServiceB:BaseUrl"]
        ?? throw new InvalidOperationException("ServiceB:BaseUrl is not configured.");
    var apiKey = builder.Configuration["ServiceB:ApiKey"]
        ?? throw new InvalidOperationException("ServiceB:ApiKey is not configured. Use dotnet user-secrets.");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// ── Typed HTTP Client → dnd-srd API ──────────────────────────────────────────
builder.Services.AddHttpClient<IDndSrdService, DndSrdService>(client =>
{
    var baseUrl = builder.Configuration["DndSrd:BaseUrl"]
        ?? "https://localhost:7120";

    client.BaseAddress = new Uri(baseUrl);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// ── Application Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IDndContentService, DndContentService>();

// ── Ports ─────────────────────────────────────────────────────────────────────
builder.WebHost.UseUrls("https://localhost:5001", "http://localhost:5000");

var app = builder.Build();

// ── Middleware pipeline (correct order per assignment) ────────────────────────
// 1. Exception handling
app.UseMiddleware<ExceptionMiddleware>();

// 2. Static files (frontend)
app.UseStaticFiles();

// 3. CORS
app.UseCors();

// 4. HTTPS redirect
app.UseHttpsRedirection();

// 5. Routing
app.UseRouting();

// 6. Auth placeholder
app.UseAuthorization();

// 7. Endpoints
app.MapControllers();

// ── Scalar UI ────────────────────────────────────────────────────────────────
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Service A – D&D Content API";
});

// Seed initial D&D content
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.Run();