using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages
{
    [Authorize(Roles = "EMPLOYER")]
    public class CandidateProfileReviewModel : PageModel
    {
        private readonly IApplicationService _applications;
        private readonly IUserProfileService _profiles;
        private readonly IOpportunityService _opportunities;
        private readonly AppDbContext _db;

        public CandidateProfileReviewModel(
            IApplicationService applications,
            IUserProfileService profiles,
            IOpportunityService opportunities,
            AppDbContext db)
        {
            _applications = applications;
            _profiles = profiles;
            _opportunities = opportunities;
            _db = db;
        }

        public long? Id { get; set; }
        [BindProperty] public string? RejectionReason { get; set; }

        public string CandidateInitials { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string ApplicationStatus { get; set; } = string.Empty;
        public long OpportunityId { get; set; }
        public string? ResumeUrl { get; set; }
        public string? ResumeFileName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? LinkedInUrl { get; set; }

        public string OpportunityTitle { get; set; } = string.Empty;
        public string OpportunityLocation { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = string.Empty;
        public string ApplicationDate { get; set; } = string.Empty;

        public string ProfileSummary { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public string CoverLetter { get; set; } = string.Empty;
        public string? RejectionReasonDisplay { get; set; }

        public List<string> Skills { get; set; } = new();
        public List<EvidenceItem> SkillEvidence { get; set; } = new();

        public async Task OnGetAsync(long? id)
        {
            Id = id;
            if (id == null) return;
            await LoadApplicationAsync(id.Value);
        }

        public async Task<IActionResult> OnPostShortlistAsync(long id)
        {
            await _applications.UpdateStatusAsync(id, new UpdateApplicationStatusDto { Status = "SHORTLISTED" });
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(long id)
        {
            await _applications.UpdateStatusAsync(id, new UpdateApplicationStatusDto
            {
                Status = "REJECTED",
                RejectionReason = RejectionReason
            });
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAcceptAsync(long id)
        {
            await _applications.UpdateStatusAsync(id, new UpdateApplicationStatusDto { Status = "ACCEPTED" });
            return RedirectToPage(new { id });
        }

        private async Task LoadApplicationAsync(long id)
        {
            var application = await _applications.GetByIdAsync(id);
            if (application == null) return;

            CandidateName = application.CandidateName;
            CandidateEmail = application.CandidateEmail;
            CandidateInitials = GetInitials(application.CandidateName);
            ApplicationStatus = application.Status;
            OpportunityTitle = application.OpportunityTitle;
            OpportunityId = application.OpportunityId;
            CoverLetter = application.CoverLetter ?? string.Empty;
            RejectionReasonDisplay = application.RejectionReason;
            ApplicationDate = application.ApplicationDate.ToString("dd MMM yyyy, hh:mm tt");

            var opportunity = await _opportunities.GetByIdAsync(application.OpportunityId);
            if (opportunity != null)
            {
                OpportunityLocation = opportunity.Location ?? string.Empty;
                OpportunityType = opportunity.OpportunityType;
            }

            var candidate = await _profiles.GetCandidateByIdAsync(application.JobSeekerProfileId);
            if (candidate != null)
            {
                ProfileSummary = candidate.Bio ?? string.Empty;
                Education = candidate.EducationLevel ?? string.Empty;
                Institution = candidate.Institution ?? string.Empty;
                ExperienceLevel = candidate.CareerInterest ?? string.Empty;
                ResumeUrl = candidate.ResumeUrl;
                ResumeFileName = candidate.ResumeUrl != null ? Path.GetFileName(candidate.ResumeUrl) : null;
                PortfolioUrl = candidate.PortfolioUrl;
                LinkedInUrl = candidate.LinkedInUrl;
                Skills = candidate.Skills;

                SkillEvidence = await _db.SkillEvidences
                    .Include(se => se.JobSeekerSkill)
                        .ThenInclude(jss => jss.Skill)
                    .Where(se => se.JobSeekerSkill.JobSeekerProfileId == candidate.Id)
                    .OrderByDescending(se => se.Id)
                    .Select(se => new EvidenceItem
                    {
                        Title = se.JobSeekerSkill.Skill.Name + " — " + se.EvidenceType,
                        Description = se.Description ?? string.Empty
                    })
                    .ToListAsync();
            }
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : parts[0][0].ToString().ToUpper();
        }

        public class EvidenceItem
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}