using Microsoft.AspNetCore.Mvc;
using ServiceB.LlmProxy.DTOs;
using ServiceB.LlmProxy.Services;

namespace ServiceB.LlmProxy.Controllers;

/// <summary>Accepts generation requests from Service A and proxies them to HuggingFace.</summary>
[ApiController]
[Route("api/[controller]")]
public class LlmController : ControllerBase
{
    private readonly IHuggingFaceService _huggingFace;
    private readonly IConfiguration _configuration;

    public LlmController(IHuggingFaceService huggingFace, IConfiguration configuration)
    {
        _huggingFace = huggingFace;
        _configuration = configuration;
    }

    /// <summary>Generates text from a prompt using the configured LLM.</summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(LlmResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Generate([FromBody] LlmRequestDto dto)
    {
        var expectedKey = _configuration["ServiceB:ApiKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
            return StatusCode(500, "Service B API key is not configured.");

        if (!Request.Headers.TryGetValue("X-Api-Key", out var incomingKey)
            || incomingKey != expectedKey)
        {
            return Unauthorized(new { message = "Invalid or missing API key." });
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var generatedText = await _huggingFace.GenerateAsync(dto.Prompt);

        return Ok(new LlmResponseDto { GeneratedText = generatedText });
    }
}