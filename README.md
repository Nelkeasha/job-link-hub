# JobLink Hub

> **A smart digital platform bridging young talent and career opportunities** — built with ASP.NET Core 8, Entity Framework Core, and SQL Server.

JobLink Hub shifts recruitment away from traditional CV-based screening toward **skills, potential, and career readiness** — helping students, graduates, and emerging professionals connect with jobs, internships, mentorships, and freelance opportunities.

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Team](#team)
- [Development Workflow](#development-workflow)
- [Branching Strategy](#branching-strategy)
- [Weekly Progress](#weekly-progress)

---

## Overview

### The Problem
Many skilled graduates fail to secure opportunities because most platforms rely on formal work experience and CV screening — disadvantaging people with strong practical skills but limited professional exposure.

### The Solution
JobLink Hub lets candidates showcase **verified skills with evidence** (certificates, portfolios, project links) and matches them to opportunities based on what they can actually do.

### User Roles
| Role | Description |
|---|---|
| **Job Seeker** | Browse opportunities, build skill profiles, upload evidence, apply |
| **Employer** | Post opportunities, search candidates by skill, review applications |
| **Admin** | Manage users, moderate content, view platform analytics |

---

## Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API framework |
| Entity Framework Core 8 | ORM and database migrations |
| SQL Server / LocalDB | Relational database |
| ASP.NET Identity | Authentication and user management |
| JWT Bearer Tokens | Stateless API authentication |
| Serilog | Structured logging |
| Swagger / OpenAPI | API documentation and testing |

### Frontend
| Technology | Purpose |
|---|---|
| Razor Pages | Server-side UI rendering |
| TailwindCSS | Utility-first responsive styling |
| HTML5 / CSS3 | Markup and layout |

---

## Project Structure

```
JobLinkHub.sln
│
├── JobLinkHub.API/                  # HTTP layer — controllers, middleware, program entry
│   ├── Controllers/                 # API endpoint controllers
│   ├── Middleware/                  # Error handling, request logging
│   ├── Extensions/                  # Service registration helpers
│   ├── appsettings.json             # Configuration (connection string, JWT)
│   └── Program.cs                   # App startup and service configuration
│
├── JobLinkHub.Data/                 # Data access layer
│   ├── Entities/                    # EF Core entity models
│   │   ├── User.cs
│   │   ├── JobSeekerProfile.cs
│   │   ├── EmployerProfile.cs
│   │   ├── Skill.cs
│   │   ├── JobSeekerSkill.cs
│   │   ├── SkillEvidence.cs
│   │   ├── Opportunity.cs
│   │   ├── OpportunitySkill.cs
│   │   ├── Application.cs
│   │   ├── SavedJob.cs
│   │   └── Notification.cs
│   ├── Repositories/                # Repository pattern implementation
│   │   └── Interfaces/              # Repository contracts
│   ├── Migrations/                  # EF Core auto-generated migrations
│   ├── AppDbContext.cs              # DbContext — tables, relationships, indexes
│   └── SeedData.cs                  # Initial roles, admin user, skills
│
├── JobLinkHub.Services/             # Business logic layer
│   ├── Interfaces/                  # Service contracts
│   ├── Implementations/             # Service implementations
│   ├── DTOs/                        # Data transfer objects
│   └── Mappings/                    # Object mapping profiles
│
└── JobLinkHub.Web/                  # Presentation layer — Razor Pages UI
    ├── Pages/                       # All Razor Pages
    └── wwwroot/                     # Static assets (CSS, JS, images)
```

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or VS Code)
- [SQL Server Express / LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Git](https://git-scm.com/)

### 1. Clone the Repository
```bash
git clone https://github.com/your-org/joblink-hub.git
cd joblink-hub
```

### 2. Configure the Database Connection
Open `JobLinkHub.API/appsettings.json` and update the connection string if needed:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobLinkHub;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Configure JWT Settings
In the same `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "JobLinkHub-Super-Secret-Key-AtLeast-32-Chars!!",
    "Issuer": "JobLinkHubAPI",
    "Audience": "JobLinkHubUsers",
    "ExpirationMinutes": 60
  }
}
```
> ⚠️ **Never commit real secret keys.** Use environment variables or user secrets in production.

### 4. Run the Application
Open **Package Manager Console** in Visual Studio and run:
```powershell
Update-Database -Project JobLinkHub.Data -StartupProject JobLinkHub.API
```

Then press **F5** or run:
```bash
dotnet run --project JobLinkHub.API
```

### 5. Access Swagger UI
Navigate to:
```
https://localhost:{port}/swagger
```

### Default Admin Account
After first run, the database is seeded with:
```
Email:    admin@joblinkhub.com
Password: Admin@12345
```
> ⚠️ Change this password immediately in any non-development environment.

---

## Database Schema

### Core Entities and Relationships

```
Users (AspNetUsers)
 ├── one-to-one → JobSeekerProfiles
 │                 ├── one-to-many → JobSeekerSkills
 │                 │                  └── one-to-many → SkillEvidences
 │                 ├── one-to-many → Applications
 │                 └── one-to-many → SavedJobs
 │
 ├── one-to-one → EmployerProfiles
 │                 └── one-to-many → Opportunities
 │                                    ├── one-to-many → Applications
 │                                    ├── one-to-many → SavedJobs
 │                                    └── many-to-many → Skills (via OpportunitySkills)
 │
 └── one-to-many → Notifications

Skills
 ├── one-to-many → JobSeekerSkills  (candidate skill proficiency)
 └── many-to-many → Opportunities   (required skills per opportunity)
```

---

## API Endpoints

### Authentication
```
POST   /api/auth/register           Register as candidate or employer
POST   /api/auth/login              Login and receive JWT token
POST   /api/auth/logout             Invalidate session
POST   /api/auth/forgot-password    Initiate password reset
GET    /api/auth/profile            Get current user profile
```

### Opportunities
```
GET    /api/opportunities           List all (filter by type, location, skill)
GET    /api/opportunities/{id}      Get opportunity detail
POST   /api/opportunities           Create opportunity (Employer only)
PUT    /api/opportunities/{id}      Update opportunity (Employer only)
DELETE /api/opportunities/{id}      Delete opportunity (Employer only)
GET    /api/opportunities/{id}/applicants   View applicants (Employer only)
```

### Applications
```
POST   /api/opportunities/{id}/apply        Apply for opportunity (Candidate)
GET    /api/applications                    View my applications
PUT    /api/applications/{id}/status        Update status (Employer only)
DELETE /api/applications/{id}               Withdraw application (Candidate)
```

### Users & Profiles
```
GET    /api/users/candidates        Browse all candidates
GET    /api/users/candidates/{id}   View candidate profile
GET    /api/users/employers         Browse all employers
GET    /api/users/employers/{id}    View employer profile
PUT    /api/users/profile           Update my profile
```

### Saved Jobs
```
GET    /api/saved-jobs              View my saved opportunities
POST   /api/saved-jobs/{id}         Save an opportunity
DELETE /api/saved-jobs/{id}         Remove from saved
```

### Dashboard
```
GET    /api/dashboard/candidate/stats       Candidate statistics
GET    /api/dashboard/employer/stats        Employer statistics
GET    /api/dashboard/admin/stats           Platform-wide statistics
```

---

## Team

| Member | Role | Responsibilities |
|---|---|---|
| **Backend Dev 1** | Database & Core API | DB schema, EF Core migrations, Opportunities API, Applications API, deployment |
| **Backend Dev 2** | Auth & Services | Authentication, JWT, business logic services, notifications, recommendations |
| **Frontend Dev 1** | Public Pages | Home page, job listings, search, candidate/employer profiles, auth pages |
| **Frontend Dev 2** | Dashboards & UI | Candidate dashboard, employer dashboard, admin panel, shared components |

---

## Development Workflow

### Daily Standup
Every day, each team member answers:
1. What did I finish yesterday?
2. What am I working on today?
3. Any blockers?

### Pull Request Rules
- No one merges their own PR
- Backend PRs reviewed by the other backend developer
- Frontend PRs reviewed by the other frontend developer
- Any change to `AppDbContext` or shared entities requires all 4 to review

### Adding a New Migration
After changing any entity in `JobLinkHub.Data/Entities/`:
```powershell
Add-Migration YourMigrationName -Project JobLinkHub.Data -StartupProject JobLinkHub.API
Update-Database -Project JobLinkHub.Data -StartupProject JobLinkHub.API
```
> Always communicate with the team before running migrations — coordinate with Backend Dev 1.

---

## Branching Strategy

```
main          ← stable, deployable code only
develop       ← active integration branch

feature/be1-*    ← Backend Dev 1 branches  e.g. feature/be1-opportunities-api
feature/be2-*    ← Backend Dev 2 branches  e.g. feature/be2-auth-service
feature/fe1-*    ← Frontend Dev 1 branches e.g. feature/fe1-job-listing-page
feature/fe2-*    ← Frontend Dev 2 branches e.g. feature/fe2-employer-dashboard
```

### Workflow
```bash
# Start a new feature
git checkout develop
git pull origin develop
git checkout -b feature/be1-opportunities-api

# Work, commit regularly
git add .
git commit -m "Add opportunity listing endpoint with filters"

# Push and open a Pull Request to develop
git push origin feature/be1-opportunities-api
```

---

## Weekly Progress

| Week | Focus | Status |
|---|---|---|
| Week 1 | Project setup, database design, entities, migrations | ✅ Complete |
| Week 2 | Repository pattern, Opportunities API, Applications API | 🔄 In Progress |
| Week 3 | Auth endpoints, User profile services, Public pages | ⏳ Pending |
| Week 4 | Candidate dashboard, Employer dashboard | ⏳ Pending |
| Week 5 | Admin dashboard, shared UI components | ⏳ Pending |
| Week 6 | Email notifications, advanced search, job alerts | ⏳ Pending |
| Week 7 | Testing, optimization, deployment | ⏳ Pending |

---

## License

This project is developed as part of an academic/portfolio project.

---

*Built with ❤️ by the JobLink Hub team*