using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Data.Entities;

namespace JobLinkHub.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signIn;
        public LogoutModel(SignInManager<User> signIn) { _signIn = signIn; }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signIn.SignOutAsync();
            return RedirectToPage("/Auth/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signIn.SignOutAsync();
            return RedirectToPage("/Auth/Login");
        }
    }
}
