using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class MyApplicationsModel : PageModel
    {
        private readonly IApplicationService _appService;
        private readonly IUserProfileService _profiles;

        public MyApplicationsModel(IApplicationService appService, IUserProfileService profiles)
        {
            _appService = appService;
            _profiles = profiles;
        }

        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
        public List<ApplicationDto> Applications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile == null) return;

            var all = await _appService.GetByJobSeekerAsync(profile.Id);

            Applications = (string.IsNullOrWhiteSpace(FilterStatus)
                ? all
                : all.Where(a => a.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<IActionResult> OnPostWithdrawAsync(long appId)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage();
            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile != null)
                await _appService.WithdrawAsync(appId, profile.Id);
            return RedirectToPage();
        }
    }
}