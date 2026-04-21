using Hangfire;
using JobLinkHub.API;
using JobLinkHub.API.Services;
using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.Implementations;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/joblinkhub-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = true;
    options.User.RequireUniqueEmail         = true;
    options.SignIn.RequireConfirmedEmail     = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme       = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

// Repositories
builder.Services.AddScoped<IOpportunityRepository,   OpportunityRepository>();
builder.Services.AddScoped<IApplicationRepository,    ApplicationRepository>();
builder.Services.AddScoped<ISavedJobRepository,       SavedJobRepository>();
builder.Services.AddScoped<INotificationRepository,   NotificationRepository>();

// Services
builder.Services.AddScoped<IOpportunityService,       OpportunityService>();
builder.Services.AddScoped<IApplicationService,        ApplicationService>();
builder.Services.AddScoped<ISavedJobService,           SavedJobService>();
builder.Services.AddScoped<IAuthService,               AuthService>();
builder.Services.AddScoped<IDashboardService,          DashboardService>();
builder.Services.AddScoped<IEmailService,              EmailService>();
builder.Services.AddScoped<INotificationService,       NotificationService>();
builder.Services.AddScoped<IRecommendationService,     RecommendationService>();
builder.Services.AddScoped<IBackgroundJobService,      BackgroundJobService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JobLink Hub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id   = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (admin only in production)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();

app.MapGet("/test-auth", (HttpContext ctx) => Results.Ok(new
{
    isAuthenticated = ctx.User.Identity?.IsAuthenticated,
    name            = ctx.User.Identity?.Name,
    claims          = ctx.User.Claims.Select(c => new { c.Type, c.Value })
})).RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// Register recurring Hangfire jobs
RecurringJob.AddOrUpdate<IBackgroundJobService>(
    "deadline-reminders",
    x => x.SendDeadlineRemindersAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<IBackgroundJobService>(
    "weekly-recommendations",
    x => x.SendWeeklyRecommendationsAsync(),
    Cron.Weekly);

RecurringJob.AddOrUpdate<IBackgroundJobService>(
    "cleanup-tokens",
    x => x.CleanupExpiredTokensAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<IBackgroundJobService>(
    "application-followup",
    x => x.SendApplicationFollowUpAsync(),
    Cron.Daily);

app.Run();
