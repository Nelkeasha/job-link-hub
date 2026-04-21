using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto);
    Task<AuthResponseDto> RegisterEmployerAsync(RegisterEmployerDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null);
    Task<bool> ChangePasswordAsync(long userId, ChangePasswordDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task RevokeTokenAsync(string refreshToken, string ipAddress);
    Task LogoutAsync(string refreshToken, string ipAddress);
    Task<bool> SendEmailVerificationAsync(long userId);
    Task<bool> VerifyEmailAsync(string userId, string token);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
}
