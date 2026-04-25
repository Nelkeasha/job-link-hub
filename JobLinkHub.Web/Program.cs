using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.Implementations;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity (cookie-based for Razor Pages)
builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // relaxed for web UI
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Repositories
builder.Services.AddScoped<IOpportunityRepository,  OpportunityRepository>();
builder.Services.AddScoped<IApplicationRepository,   ApplicationRepository>();
builder.Services.AddScoped<ISavedJobRepository,      SavedJobRepository>();
builder.Services.AddScoped<INotificationRepository,  NotificationRepository>();
builder.Services.AddScoped<IUserProfileRepository,   UserProfileRepository>();
builder.Services.AddScoped<ISkillRepository,         SkillRepository>();

// Services
builder.Services.AddScoped<IOpportunityService,      OpportunityService>();
builder.Services.AddScoped<IApplicationService,       ApplicationService>();
builder.Services.AddScoped<ISavedJobService,          SavedJobService>();
builder.Services.AddScoped<IDashboardService,         DashboardService>();
builder.Services.AddScoped<IEmailService,             EmailService>();
builder.Services.AddScoped<INotificationService,      NotificationService>();
builder.Services.AddScoped<IRecommendationService,    RecommendationService>();
builder.Services.AddScoped<IUserProfileService,       UserProfileService>();
builder.Services.AddScoped<ISkillService,             SkillService>();
builder.Services.AddScoped<IAdminService,             AdminService>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Public/Index", "");
});
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

// Seed roles and default admin account
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
