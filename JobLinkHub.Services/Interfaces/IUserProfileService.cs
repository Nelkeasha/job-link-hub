using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IUserProfileService
{
    Task<CandidateProfileDto?> GetCandidateByUserIdAsync(long userId);
    Task<CandidateProfileDto?> GetCandidateByIdAsync(long id);
    Task<IEnumerable<CandidateProfileDto>> GetAllCandidatesAsync(string? keyword, string? location);
    Task<CandidateProfileDto> UpdateCandidateAsync(long userId, UpdateCandidateProfileDto dto);

    Task<EmployerProfileDto?> GetEmployerByUserIdAsync(long userId);
    Task<EmployerProfileDto?> GetEmployerByIdAsync(long id);
    Task<IEnumerable<EmployerProfileDto>> GetAllEmployersAsync(string? keyword, string? location);
    Task<EmployerProfileDto> UpdateEmployerAsync(long userId, UpdateEmployerProfileDto dto);
}
