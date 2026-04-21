using Hangfire.Dashboard;

namespace JobLinkHub.API;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow all access in development
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
            return true;

        // In production, require ADMIN role
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("ADMIN");
    }
}
