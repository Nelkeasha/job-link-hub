using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly IUserProfileService _profiles;
        private readonly ISkillService _skills;

        public IndexModel(IOpportunityService opportunities, IUserProfileService profiles, ISkillService skills)
        {
            _opportunities = opportunities;
            _profiles = profiles;
            _skills = skills;
        }

        public List<OpportunityDto> LatestOpportunities { get; set; } = new();
        public List<EmployerProfileDto> TopCompanies { get; set; } = new();
        public List<CategoryCountDto> TopCategories { get; set; } = new();

        public class CategoryCountDto
        {
            public string CategoryName { get; set; } = string.Empty;
            public int Count { get; set; }
            public string IconClass { get; set; } = string.Empty;
            public string ColorClass { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            // Fetch active opportunities
            var allOpp = (await _opportunities.GetAllAsync(null, null, null, "Active")).ToList();
            LatestOpportunities = allOpp.Take(6).ToList(); // Get top 6

            // Fetch top companies
            var allEmp = await _profiles.GetAllEmployersAsync(null, null);
            TopCompanies = allEmp.OrderByDescending(e => e.ActiveOpportunitiesCount).Take(4).ToList();

            // Fetch categories and count opportunities
            var groupedSkills = await _skills.GetGroupedByCategoryAsync();
            var categoriesList = new List<CategoryCountDto>();

            var iconMap = new[]
            {
                ("fa-graduation-cap", "cat-blue"),
                ("fa-gear", "cat-amber"),
                ("fa-chart-pie", "cat-green"),
                ("fa-heart-pulse", "cat-rose"),
                ("fa-bullhorn", "cat-purple"),
                ("fa-money-bill-trend-up", "cat-teal"),
                ("fa-microchip", "cat-indigo"),
                ("fa-ellipsis", "cat-gray")
            };

            int index = 0;
            foreach (var group in groupedSkills)
            {
                if (string.IsNullOrEmpty(group.Category)) continue;

                var skillNamesInCat = group.Skills.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                
                // An opportunity belongs to this category if any of its required skills are in this category
                int oppCount = allOpp.Count(o => o.RequiredSkills.Any(rs => skillNamesInCat.Contains(rs)));

                var (icon, color) = iconMap[index % iconMap.Length];
                
                categoriesList.Add(new CategoryCountDto
                {
                    CategoryName = group.Category,
                    Count = oppCount,
                    IconClass = icon,
                    ColorClass = color
                });
                index++;
            }

            TopCategories = categoriesList.OrderByDescending(c => c.Count).Take(8).ToList();
        }
    }
}
