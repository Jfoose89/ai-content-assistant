using ServiceA.ContentApi.DTOs;

namespace ServiceA.ContentApi.Services;

/// <summary>
/// Typed HTTP client that forwards generation requests to Service B (LLM Proxy).
/// Handles timeouts and upstream error responses explicitly.
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

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/llm/generate", request);
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("The request to the AI service timed out.");
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("Could not reach the AI service.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var status = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(body, null, response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<GenerateResponseDto>();
        return result ?? new GenerateResponseDto { GeneratedText = string.Empty };
    }
}