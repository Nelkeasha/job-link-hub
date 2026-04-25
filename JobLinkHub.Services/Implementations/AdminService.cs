using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public AdminService(UserManager<User> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IEnumerable<UserListDto>> GetAllUsersAsync(string? role, string? keyword)
    {
        var users = await _context.Users
            .Include(u => u.JobSeekerProfile)
            .Include(u => u.EmployerProfile)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(role))
            users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(keyword))
            users = users.Where(u =>
                u.Email!.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        return users.Select(u => new UserListDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            IsActive = u.IsActive,
            EmailConfirmed = u.EmailConfirmed,
            CreatedAt = u.CreatedAt,
            ProfileId = u.Role == "CANDIDATE"
                ? u.JobSeekerProfile?.Id
                : u.EmployerProfile?.Id
        });
    }

    public async Task<UserListDto?> GetUserByIdAsync(long id)
    {
        var user = await _context.Users
            .Include(u => u.JobSeekerProfile)
            .Include(u => u.EmployerProfile)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        return new UserListDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            ProfileId = user.Role == "CANDIDATE"
                ? user.JobSeekerProfile?.Id
                : user.EmployerProfile?.Id
        };
    }

    public async Task<bool> SetUserActiveStatusAsync(long userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<IEnumerable<AdminOpportunityDto>> GetAllOpportunitiesForModerationAsync()
    {
        var opps = await _context.Opportunities
            .Include(o => o.EmployerProfile)
            .Include(o => o.Applications)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return opps.Select(o => new AdminOpportunityDto
        {
            Id = o.Id,
            Title = o.Title,
            CompanyName = o.EmployerProfile.CompanyName,
            Status = o.Status,
            OpportunityType = o.OpportunityType,
            Views = o.Views,
            ApplicationCount = o.Applications.Count,
            CreatedAt = o.CreatedAt
        });
    }

    public async Task<bool> DeleteOpportunityAsync(long opportunityId)
    {
        var opp = await _context.Opportunities.FindAsync(opportunityId);
        if (opp == null) return false;
        _context.Opportunities.Remove(opp);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleOpportunityStatusAsync(long opportunityId, string currentStatus)
    {
        var opp = await _context.Opportunities.FindAsync(opportunityId);
        if (opp == null) return false;
        opp.Status = currentStatus == "Active" ? "Closed" : "Active";
        await _context.SaveChangesAsync();
        return true;
    }
}
