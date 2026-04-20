using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobLinkHub.Web.Pages
{
    public class CompanyProfileModel : PageModel
    {
        public string CompanyInitials { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyType { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string CompanyDescription { get; set; } = string.Empty;

        public void OnGet()
        {
            // Temporary mock data for frontend development.
            // Backend can later replace this with real company profile data.
            CompanyInitials = "AS";
            CompanyName = "Acme Studio";
            CompanyType = "Private Company";
            Industry = "Technology";
            Location = "Kigali, Rwanda";
            Website = "www.acmestudio.com";
            ContactEmail = "careers@acmestudio.com";
            CompanyDescription = "Acme Studio is a forward-looking technology company focused on building digital products and creating meaningful opportunities for emerging talent. We value innovation, practical skills, and a collaborative working culture.";
        }
    }
}