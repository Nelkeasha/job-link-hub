using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto);
    Task<AuthResponseDto> RegisterEmployerAsync(RegisterEmployerDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<bool> ChangePasswordAsync(long userId, ChangePasswordDto dto);
}