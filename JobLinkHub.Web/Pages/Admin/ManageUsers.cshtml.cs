using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ManageUsersModel : PageModel
    {
        private readonly IAdminService _admin;
        public ManageUsersModel(IAdminService admin) { _admin = admin; }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterRole { get; set; }

        public List<UserListDto> Users { get; set; } = new();
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            Users = (await _admin.GetAllUsersAsync(FilterRole, SearchQ)).ToList();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(long userId, bool isActive)
        {
            await _admin.SetUserActiveStatusAsync(userId, !isActive);
            return RedirectToPage(new { SearchQ, FilterRole });
        }

        public async Task<IActionResult> OnPostDeleteAsync(long userId)
        {
            await _admin.DeleteUserAsync(userId);
            return RedirectToPage(new { SearchQ, FilterRole });
        }
    }
}