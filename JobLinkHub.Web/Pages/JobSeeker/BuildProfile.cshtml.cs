using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;
using System.ComponentModel.DataAnnotations;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class BuildProfileModel : PageModel
    {
        private readonly IUserProfileService _profiles;
        private readonly ISkillService _skills;
        private readonly IWebHostEnvironment _env;

        public BuildProfileModel(IUserProfileService profiles, ISkillService skills, IWebHostEnvironment env)
        {
            _profiles = profiles;
            _skills = skills;
            _env = env;
        }

        [BindProperty, Required(ErrorMessage = "First name is required"), StringLength(50)]
        public string FirstName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Last name is required"), StringLength(50)]
        public string LastName { get; set; } = "";

        [BindProperty, StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string? Bio { get; set; }

        [BindProperty] public string? Location { get; set; }
        [BindProperty] public string? PhoneNumber { get; set; }
        [BindProperty] public string? LinkedInUrl { get; set; }
        [BindProperty] public string? PortfolioUrl { get; set; }
        [BindProperty] public string? CareerInterest { get; set; }
        [BindProperty] public string? EducationLevel { get; set; }
        [BindProperty] public string? Institution { get; set; }

        [BindProperty, Range(1950, 2030, ErrorMessage = "Please enter a valid graduation year (1950-2030)")]
        public int? GraduationYear { get; set; }
        [BindProperty] public List<long> SelectedSkillIds { get; set; } = new();
        [BindProperty] public IFormFile? ResumeFile { get; set; }

        public string? ExistingResumeUrl { get; set; }
        public string? ExistingResumeFileName { get; set; }
        public List<SkillCategoryDto> SkillsByCategory { get; set; } = new();
        public List<string> CurrentSkills { get; set; } = new();
        public string? Message { get; set; }
        public bool Success { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return Page();

            try
            {
                var currentProfile = await _profiles.GetCandidateByUserIdAsync(userId);
                var resumeUrl = currentProfile?.ResumeUrl;

                if (ResumeFile != null && ResumeFile.Length > 0)
                {
                    var uploadedUrl = await SaveResumeAsync(ResumeFile, userId);
                    if (uploadedUrl == null)
                    {
                        Message = "Invalid file. Please upload a PDF, DOC, or DOCX file (max 5 MB).";
                        await LoadAsync();
                        return Page();
                    }
                    resumeUrl = uploadedUrl;
                }

                await _profiles.UpdateCandidateAsync(userId, new UpdateCandidateProfileDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber,
                    Bio = Bio,
                    CareerInterest = CareerInterest,
                    EducationLevel = EducationLevel,
                    Institution = Institution,
                    GraduationYear = GraduationYear,
                    LinkedInUrl = LinkedInUrl,
                    PortfolioUrl = PortfolioUrl,
                    ResumeUrl = resumeUrl,
                    Location = Location,
                    SkillIds = SelectedSkillIds
                });

                Success = true;
                Message = "Profile saved successfully!";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }

            await LoadAsync();
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

        private async Task LoadAsync()
        {
            SkillsByCategory = (await _skills.GetGroupedByCategoryAsync()).ToList();

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile == null) return;

            FirstName = profile.FirstName;
            LastName = profile.LastName;
            PhoneNumber = profile.PhoneNumber;
            Bio = profile.Bio;
            Location = profile.Location;
            CareerInterest = profile.CareerInterest;
            EducationLevel = profile.EducationLevel;
            Institution = profile.Institution;
            GraduationYear = profile.GraduationYear;
            LinkedInUrl = profile.LinkedInUrl;
            PortfolioUrl = profile.PortfolioUrl;
            ExistingResumeUrl = profile.ResumeUrl;
            ExistingResumeFileName = profile.ResumeUrl != null
                ? Path.GetFileName(profile.ResumeUrl)
                : null;
            CurrentSkills = profile.Skills;
            SelectedSkillIds = profile.SkillIds;
        }
    }
}
