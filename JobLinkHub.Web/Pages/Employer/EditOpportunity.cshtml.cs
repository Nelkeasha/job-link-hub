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
    public class EditOpportunityModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly IUserProfileService _profiles;

        public EditOpportunityModel(IOpportunityService opportunities, IUserProfileService profiles)
        {
            _opportunities = opportunities;
            _profiles = profiles;
        }

        [BindProperty(SupportsGet = true)] public long Id { get; set; }

        [BindProperty, Required(ErrorMessage = "Title is required"), StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [BindProperty] public string OpportunityType { get; set; } = "Job";
        [BindProperty] public string? Location { get; set; }

        [BindProperty, FutureDate]
        public DateTime? Deadline { get; set; }

        [BindProperty] public string? QualificationRequired { get; set; }

        [BindProperty, Required(ErrorMessage = "Description is required"), StringLength(5000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters")]
        public string? Description { get; set; }

        [BindProperty] public string Status { get; set; } = "Active";

        public string? Message { get; set; }
        public new bool NotFound { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var opp = await _opportunities.GetByIdAsync(Id);
            if (opp == null) { NotFound = true; return Page(); }

            Title = opp.Title;
            OpportunityType = opp.OpportunityType;
            Location = opp.Location;
            Deadline = opp.Deadline;
            QualificationRequired = opp.QualificationRequired;
            Description = opp.Description;
            Status = opp.Status;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
            {
                Message = "Unable to identify user.";
                return Page();
            }

            var profile = await _profiles.GetEmployerByUserIdAsync(userId);
            if (profile == null)
            {
                Message = "Employer profile not found.";
                return Page();
            }

            var dto = new UpdateOpportunityDto
            {
                Title = Title,
                Description = Description ?? string.Empty,
                OpportunityType = OpportunityType,
                Location = Location,
                QualificationRequired = QualificationRequired,
                Deadline = Deadline,
                Status = Status
            };

            var result = await _opportunities.UpdateAsync(Id, profile.Id, dto);
            if (result == null)
            {
                Message = "Opportunity not found or you do not have permission to edit it.";
                return Page();
            }

            TempData["SuccessMessage"] = "Opportunity updated successfully!";
            return RedirectToPage("/Employer/ManageOpportunities");
        }
    }
}
