using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobLinkHub.Web.Pages.Public
{
    public class ContactModel : PageModel
    {
        [BindProperty] public string Name { get; set; } = "";
        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Subject { get; set; } = "";
        [BindProperty] public string Message { get; set; } = "";

        public bool Submitted { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Message))
            {
                ErrorMessage = "Please fill in all required fields.";
                return Page();
            }
            // In production this would send an email via IEmailService
            Submitted = true;
            return Page();
        }
    }
}
