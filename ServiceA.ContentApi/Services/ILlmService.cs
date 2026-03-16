using ServiceA.ContentApi.DTOs;

namespace ServiceA.ContentApi.Services;

public interface ILlmService
{
    Task<GenerateResponseDto> GenerateAsync(string prompt);
}