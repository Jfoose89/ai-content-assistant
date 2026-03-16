using System.ComponentModel.DataAnnotations;

namespace ServiceB.LlmProxy.DTOs;

/// <summary>Request body received from Service A.</summary>
public class LlmRequestDto
{
    /// <summary>The prompt to send to the LLM.</summary>
    [Required]
    public string Prompt { get; set; } = string.Empty;
}

/// <summary>Response returned to Service A.</summary>
public class LlmResponseDto
{
    public string GeneratedText { get; set; } = string.Empty;
}

/// <summary>Internal: HuggingFace Inference API request body.</summary>
public class HuggingFaceRequestDto
{
    public string inputs { get; set; } = string.Empty;
    public HuggingFaceParameters parameters { get; set; } = new();
}

public class HuggingFaceParameters
{
    public int max_new_tokens { get; set; } = 512;
    public bool return_full_text { get; set; } = false;
}

/// <summary>Internal: HuggingFace Inference API response body.</summary>
public class HuggingFaceResponseDto
{
    public string generated_text { get; set; } = string.Empty;
}