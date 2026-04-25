using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class OpportunityDetailsModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly IApplicationService _applications;
        private readonly IUserProfileService _profiles;

        public OpportunityDetailsModel(
            IOpportunityService opportunities,
            IApplicationService applications,
            IUserProfileService profiles)
        {
            _opportunities = opportunities;
            _applications = applications;
            _profiles = profiles;
        }

        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? EmployerDescription { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public bool AlreadyApplied { get; set; }
        public string? NotFoundMessage { get; set; }

        public async Task OnGetAsync(long id)
        {
            var opportunity = await _opportunities.GetByIdAsync(id);
            if (opportunity == null)
            {
                NotFoundMessage = "Opportunity not found.";
                return;
            }

            Id = opportunity.Id;
            Title = opportunity.Title;
            CompanyName = opportunity.CompanyName;
            Type = opportunity.OpportunityType;
            Location = opportunity.Location;
            Description = opportunity.Description;
            Deadline = opportunity.Deadline;
            RequiredSkills = opportunity.RequiredSkills;

            // Check if the current user has already applied
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out var userId))
            {
                var profile = await _profiles.GetCandidateByUserIdAsync(userId);
                if (profile != null)
                {
                    AlreadyApplied = await _applications.HasAppliedAsync(profile.Id, id);
                }
            }

            // Increment view count
            await _opportunities.IncrementViewsAsync(id);
        }
    }
}