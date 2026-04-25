using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class ApplyModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly IApplicationService _applications;
        private readonly IUserProfileService _profiles;
        private readonly IWebHostEnvironment _env;

        public ApplyModel(
            IOpportunityService opportunities,
            IApplicationService applications,
            IUserProfileService profiles,
            IWebHostEnvironment env)
        {
            _opportunities = opportunities;
            _applications = applications;
            _profiles = profiles;
            _env = env;
        }

        [BindProperty] public string? CoverNote { get; set; }
        [BindProperty] public IFormFile? ResumeFile { get; set; }
        [BindProperty] public long OpportunityId { get; set; }

        public string JobTitle { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string ApplicantName { get; set; } = "";
        public string ApplicantEmail { get; set; } = "";
        public string? ApplicantLocation { get; set; }
        public string? ExistingResumeUrl { get; set; }
        public string? ExistingResumeFileName { get; set; }
        public bool ProfileIncomplete { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
        public string? NotFoundMessage { get; set; }

        public async Task OnGetAsync(long id)
        {
            OpportunityId = id;

            var opportunity = await _opportunities.GetByIdAsync(id);
            if (opportunity == null) { NotFoundMessage = "Opportunity not found."; return; }
            JobTitle = opportunity.Title;
            CompanyName = opportunity.CompanyName;

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var candidate = await _profiles.GetCandidateByUserIdAsync(userId);
            if (candidate != null)
            {
                ApplicantName = $"{candidate.FirstName} {candidate.LastName}".Trim();
                ApplicantEmail = candidate.Email;
                ApplicantLocation = candidate.Location;
                ExistingResumeUrl = candidate.ResumeUrl;
                ExistingResumeFileName = candidate.ResumeUrl != null
                    ? Path.GetFileName(candidate.ResumeUrl)
                    : null;
                ProfileIncomplete = string.IsNullOrEmpty(candidate.Bio)
                    || string.IsNullOrEmpty(candidate.EducationLevel)
                    || !candidate.Skills.Any();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return Page();

            try
            {
                var candidate = await _profiles.GetCandidateByUserIdAsync(userId);
                if (candidate == null) { Message = "Please complete your profile before applying."; return Page(); }

                if (await _applications.HasAppliedAsync(candidate.Id, OpportunityId))
                {
                    Message = "You have already applied for this opportunity.";
                    await ReloadPageDataAsync(userId);
                    return Page();
                }

                // Resolve resume: new upload > existing on profile
                string? resumeUrl = candidate.ResumeUrl;

                if (ResumeFile != null && ResumeFile.Length > 0)
                {
                    var uploaded = await SaveResumeAsync(ResumeFile, userId);
                    if (uploaded == null)
                    {
                        Message = "Invalid file. Please upload a PDF, DOC, or DOCX (max 5 MB).";
                        await ReloadPageDataAsync(userId);
                        return Page();
                    }
                    resumeUrl = uploaded;

                    // Persist the newly uploaded resume to the candidate profile
                    await _profiles.UpdateCandidateAsync(userId, new UpdateCandidateProfileDto
                    {
                        FirstName = candidate.FirstName,
                        LastName = candidate.LastName,
                        PhoneNumber = candidate.PhoneNumber,
                        Bio = candidate.Bio,
                        CareerInterest = candidate.CareerInterest,
                        EducationLevel = candidate.EducationLevel,
                        Institution = candidate.Institution,
                        GraduationYear = candidate.GraduationYear,
                        LinkedInUrl = candidate.LinkedInUrl,
                        PortfolioUrl = candidate.PortfolioUrl,
                        Location = candidate.Location,
                        ResumeUrl = resumeUrl,
                        SkillIds = candidate.SkillIds
                    });
                }

                await _applications.CreateAsync(candidate.Id, new CreateApplicationDto
                {
                    OpportunityId = OpportunityId,
                    CoverLetter = CoverNote,
                    ResumeUsed = resumeUrl
                });

                Success = true;
                Message = "Application submitted successfully!";
            }
            catch (Exception ex) { Message = "Error: " + ex.Message; }

            var opp = await _opportunities.GetByIdAsync(OpportunityId);
            if (opp != null) { JobTitle = opp.Title; CompanyName = opp.CompanyName; }

            return Page();
        }

        private async Task<string?> SaveResumeAsync(IFormFile file, long userId)
        {
            var allowed = new[] { ".pdf", ".doc", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;
            if (file.Length > 5 * 1024 * 1024) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "resumes");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/resumes/{fileName}";
        }

        private async Task ReloadPageDataAsync(long userId)
        {
            var opp = await _opportunities.GetByIdAsync(OpportunityId);
            if (opp != null) { JobTitle = opp.Title; CompanyName = opp.CompanyName; }

            var candidate = await _profiles.GetCandidateByUserIdAsync(userId);
            if (candidate != null)
            {
                ApplicantName = $"{candidate.FirstName} {candidate.LastName}".Trim();
                ApplicantEmail = candidate.Email;
                ApplicantLocation = candidate.Location;
                ExistingResumeUrl = candidate.ResumeUrl;
                ExistingResumeFileName = candidate.ResumeUrl != null
                    ? Path.GetFileName(candidate.ResumeUrl)
                    : null;
            }
        }
    }
}
