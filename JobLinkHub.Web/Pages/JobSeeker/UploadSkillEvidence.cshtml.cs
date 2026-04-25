using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class UploadSkillEvidenceModel : PageModel
    {
        private readonly ISkillService _skills;
        private readonly IUserProfileService _profiles;
        private readonly AppDbContext _db;

        public UploadSkillEvidenceModel(ISkillService skills, IUserProfileService profiles, AppDbContext db)
        {
            _skills = skills;
            _profiles = profiles;
            _db = db;
        }

        [BindProperty] public long SkillId { get; set; }
        [BindProperty] public string? ProficiencyLevel { get; set; }
        [BindProperty] public string? EvidenceType { get; set; }
        [BindProperty] public string? EvidenceUrl { get; set; }
        [BindProperty] public string? Description { get; set; }

        public string? Message { get; set; }
        public bool Success { get; set; }
        public List<SkillDto> AvailableSkills { get; set; } = new();
        public List<EvidenceItem> MyEvidence { get; set; } = new();

        public class EvidenceItem
        {
            public long Id { get; set; }
            public string SkillName { get; set; } = "";
            public string EvidenceType { get; set; } = "";
            public string? Url { get; set; }
            public string? Description { get; set; }
            public string ProficiencyLevel { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
            {
                Message = "Unable to identify user.";
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var profile = await _profiles.GetCandidateByUserIdAsync(userId);
                if (profile == null)
                {
                    Message = "Please build your profile first.";
                    await LoadDataAsync();
                    return Page();
                }

                // Upsert JobSeekerSkill
                var jobSeekerSkill = await _db.Set<JobSeekerSkill>()
                    .FirstOrDefaultAsync(jss => jss.JobSeekerProfileId == profile.Id && jss.SkillId == SkillId);

                if (jobSeekerSkill == null)
                {
                    jobSeekerSkill = new JobSeekerSkill
                    {
                        JobSeekerProfileId = profile.Id,
                        SkillId = SkillId,
                        ProficiencyLevel = ProficiencyLevel ?? "Beginner"
                    };
                    _db.Set<JobSeekerSkill>().Add(jobSeekerSkill);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    jobSeekerSkill.ProficiencyLevel = ProficiencyLevel ?? "Beginner";
                    await _db.SaveChangesAsync();
                }

                // Insert evidence
                var evidence = new SkillEvidence
                {
                    JobSeekerSkillId = jobSeekerSkill.Id,
                    EvidenceType = EvidenceType ?? "",
                    EvidenceLink = EvidenceUrl ?? "",
                    Description = Description
                };
                _db.SkillEvidences.Add(evidence);
                await _db.SaveChangesAsync();

                Success = true;
                Message = "Evidence added successfully!";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }

            await LoadDataAsync();
            return Page();
        }

        private async Task LoadDataAsync()
        {
            AvailableSkills = (await _skills.GetAllAsync()).ToList();

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile == null) return;

            MyEvidence = await _db.SkillEvidences
                .Include(se => se.JobSeekerSkill)
                    .ThenInclude(jss => jss.Skill)
                .Where(se => se.JobSeekerSkill.JobSeekerProfileId == profile.Id)
                .OrderByDescending(se => se.Id)
                .Select(se => new EvidenceItem
                {
                    Id = se.Id,
                    SkillName = se.JobSeekerSkill.Skill.Name,
                    EvidenceType = se.EvidenceType,
                    Url = se.EvidenceLink,
                    Description = se.Description,
                    ProficiencyLevel = se.JobSeekerSkill.ProficiencyLevel ?? ""
                })
                .ToListAsync();
        }
    }
}