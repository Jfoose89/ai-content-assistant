using ServiceB.LlmProxy.DTOs;

namespace ServiceB.LlmProxy.Services;

public interface IHuggingFaceService
{
    Task<string> GenerateAsync(string prompt);
}

/// <summary>
/// Typed HTTP client that forwards prompts to the HuggingFace Inference API.
/// </summary>
public class HuggingFaceService : IHuggingFaceService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public HuggingFaceService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _model = configuration["HuggingFace:Model"]
            ?? "meta-llama/Llama-3.1-8B-Instruct";
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 512
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/v1/chat/completions",
            requestBody);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>();

        return result?.choices?.FirstOrDefault()?.message?.content
            ?? string.Empty;
    }
}

// Internal response classes for the new API format
public class ChatCompletionResponse
{
    public List<ChatChoice>? choices { get; set; }
}

public class ChatChoice
{
    public ChatMessage? message { get; set; }
}

public class ChatMessage
{
    public string? content { get; set; }
}