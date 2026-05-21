using System.Net;
using ServiceB.LlmProxy.DTOs;

namespace ServiceB.LlmProxy.Services;

public interface IHuggingFaceService
{
    Task<string> GenerateAsync(string prompt);
}

/// <summary>
/// Typed HTTP client that forwards prompts to the HuggingFace Inference API.
/// Handles timeouts, empty responses, and upstream HTTP errors explicitly.
/// </summary>
public class HuggingFaceService : IHuggingFaceService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<HuggingFaceService> _logger;

    public HuggingFaceService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HuggingFaceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
                new
                {
                    role = "system",
                    content = """
                        You are a creative assistant for Dungeons & Dragons 5th Edition.
                        Your job is to generate vivid, atmospheric content for Dungeon Masters.

                        Rules:
                        - Only generate D&D-related content. If the request is unrelated, respond with: "I can only generate D&D content."
                        - Never invent game mechanics, spell stats, or monster stats that contradict official 5e rules.
                        - If you are uncertain about official lore, say so clearly rather than guessing.
                        - Keep responses between 150 and 300 words unless instructed otherwise.
                        - Never break character or refer to yourself as an AI.

                        Output format:
                        - Use a clear heading (e.g. ## The Aboleth)
                        - Write in vivid, atmospheric prose suitable for reading aloud at the table
                        - End with a short "Dungeon Master's note" in italics with one practical gameplay tip
                        """
                },
                new { role = "user", content = prompt }
            },
            max_tokens = 512
        };

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                "/v1/chat/completions",
                requestBody);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("HuggingFace request timed out.");
            throw new HuggingFaceException("The AI service timed out. Please try again.", 504);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("HuggingFace HTTP request failed: {Message}", ex.Message);
            throw new HuggingFaceException("Could not reach the AI service.", 502);
        }

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            var reason = response.ReasonPhrase ?? "Unknown error";

            _logger.LogWarning(
                "HuggingFace returned non-success status {StatusCode}: {Reason}",
                statusCode, reason);

            var title = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "AI service authentication failed.",
                HttpStatusCode.Forbidden => "Access to the AI service was denied.",
                HttpStatusCode.TooManyRequests => "AI service rate limit reached. Please wait and try again.",
                >= HttpStatusCode.InternalServerError => "The AI service encountered an internal error.",
                _ => "The AI service returned an unexpected error."
            };

            throw new HuggingFaceException(title, statusCode);
        }

        var result = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>();

        var text = result?.choices?.FirstOrDefault()?.message?.content;

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("HuggingFace returned an empty response.");
            throw new HuggingFaceException("The AI service returned an empty response.", 502);
        }

        return text.Trim();
    }
}

// Internal response shape for HuggingFace chat completions
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

/// <summary>
/// Represents a known error from the HuggingFace API.
/// Carries the upstream HTTP status code for translation into a ProblemDetails response.
/// </summary>
public class HuggingFaceException : Exception
{
    public int StatusCode { get; }

    public HuggingFaceException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}