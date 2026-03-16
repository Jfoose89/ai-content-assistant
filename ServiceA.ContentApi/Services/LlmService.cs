using ServiceA.ContentApi.DTOs;

namespace ServiceA.ContentApi.Services;

/// <summary>
/// Typed HTTP client that forwards generation requests to Service B (LLM Proxy).
/// </summary>
public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;

    public LlmService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GenerateResponseDto> GenerateAsync(string prompt)
    {
        var request = new GenerateRequestDto { Prompt = prompt };
        var response = await _httpClient.PostAsJsonAsync("/api/llm/generate", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerateResponseDto>();
        return result ?? new GenerateResponseDto { GeneratedText = string.Empty };
    }
}