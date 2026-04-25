using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;
using System.ComponentModel.DataAnnotations;
using JobLinkHub.Web.Validation;

namespace JobLinkHub.Web.Pages.Employer
{
    [Authorize(Roles = "EMPLOYER")]
    public class PostOpportunityModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly ISkillService _skills;
        private readonly IUserProfileService _profiles;

        public PostOpportunityModel(IOpportunityService opportunities, ISkillService skills, IUserProfileService profiles)
        {
            _opportunities = opportunities;
            _skills = skills;
            _profiles = profiles;
        }

        [BindProperty, Required(ErrorMessage = "Title is required"), StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [BindProperty] public string OpportunityType { get; set; } = "Job";
        [BindProperty] public string? Location { get; set; }

        [BindProperty, FutureDate]
        public DateTime? Deadline { get; set; }

        [BindProperty] public string? QualificationRequired { get; set; }
        [BindProperty] public string? SkillsInput { get; set; }

        [BindProperty, Required(ErrorMessage = "Description is required"), StringLength(5000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters")]
        public string? Description { get; set; }

        [BindProperty] public string Status { get; set; } = "Active";

        public List<SkillDto> AvailableSkills { get; set; } = new();
        public string? Message { get; set; }
        public bool Success { get; set; }

        public async Task OnGetAsync()
        {
            AvailableSkills = (await _skills.GetAllAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableSkills = (await _skills.GetAllAsync()).ToList();
                return Page();
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
            {
                Message = "Unable to identify user.";
                return Page();
            }

            try
            {
                var profile = await _profiles.GetEmployerByUserIdAsync(userId);
                if (profile == null)
                {
                    Message = "Please complete your company profile first.";
                    return Page();
                }

                var dto = new CreateOpportunityDto
                {
                    Title = Title,
                    Description = Description ?? string.Empty,
                    OpportunityType = OpportunityType,
                    Location = Location,
                    QualificationRequired = QualificationRequired,
                    Deadline = Deadline,
                    Status = Status == "Live" ? "Active" : Status
                };

                await _opportunities.CreateAsync(profile.Id, dto);
                TempData["SuccessMessage"] = "Opportunity posted successfully!";
                return RedirectToPage("/Employer/ManageOpportunities");
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
                AvailableSkills = (await _skills.GetAllAsync()).ToList();
                return Page();
            }
        }
    }
}