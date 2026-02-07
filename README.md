# AuthManager Enterprise — ASP.NET Core MVC User Management System

AuthManager Enterprise is a lightweight ASP.NET Core MVC application for user registration, authentication, and administrative user management. It uses cookie-based authentication, Entity Framework Core, and SQL Server. The system supports registration, login, status control (verified / unverified / blocked), bulk admin actions, and global middleware-based access enforcement.

This project is structured for training, academic demos, and starter enterprise auth workflows.
## Core Capabilities

- User registration with unique email enforcement
- Cookie-based authentication
- Login / logout flow
- Admin-only user management panel
- Bulk actions on users (block, unblock, delete, delete unverified)
- Global middleware to auto-logout blocked/deleted users
- EF Core + SQL Server persistence layer
- Bootstrap UI
- ViewModel-driven forms with validation
- Select-all checkbox behavior for admin bulk operations
## Technology Stack

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Cookie Authentication
- Razor Views
- Bootstrap 5
- C#
## Database Design

### User Table
```
| Field            | Type      | Notes                       |
|-----------------|-----------|-----------------------------|
| Id              | int       | Primary Key                 |
| Name            | string    | Required                    |
| Email           | string    | Required, Unique Index      |
| Password        | string    | Required                    |
| Status          | string    | unverified / blocked / active |
| LastLoginTime   | DateTime? | Nullable                    |
| RegistrationTime| DateTime  | Default = Now               |
```
### Unique Email Constraint
Configured in `ApplicationDbContext`:

- Unique index on `Email`
- Prevents duplicate registration at database level
- Violations trigger `DbUpdateException`

## Authentication Model

Cookie-based authentication is configured in `Program.cs`:

```csharp
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
    });
```
## Request Pipeline Security Middleware

A custom middleware runs on every request:

- Skips login and registration routes
- If user is authenticated:
  - Loads user from database
  - If user is deleted or blocked:
    - Signs out immediately
    - Redirects to login

This ensures blocked users cannot remain logged in.
Claims stored in cookie:

- **Name** = Email
- **NameIdentifier** = UserId


### Registration Flow

- Validates input via ViewModel
- Creates new User with:
  - Status = unverified
  - RegistrationTime = current time
- Saves to database
- Unique email enforced at DB layer
- Redirects to Login on success

### Login Flow

- Checks Email + Password match
- Rejects blocked users
- Updates LastLoginTime
- Issues auth cookie
- Redirects to Admin dashboard

### Logout Flow

- Clears auth cookie
- Redirects to login page

## Admin Panel

### Dashboard Behavior

- Lists all users
- Sorted by last login descending
- Displays:
  - Name
  - Email
  - Last Login
  - Status badge

### Bulk Actions

Admin can select multiple users and perform:

- Block
- Unblock
- Delete
- Delete Unverified

Checkbox grid supports **Select All** toggle via JavaScript.

## ViewModels

### RegisterViewModel

- Name
- Email
- Password
- Validation attributes applied

### LoginViewModel

- Email
- Password
- RememberMe flag (UI level)

### UserManagementViewModel

- Users list
- Selected user IDs list
- Supports bulk form posting

## UI Notes

- Bootstrap-based layout
- Shared layout with auth-aware navigation
- Login/Register forms include validation summary
- Admin table supports bulk checkbox selection
- Status badges are color-coded

## Known Gaps (Intentional for Training Scope)

- Password stored in plain text (should be hashed in production)
- No role-based authorization separation yet
- No email verification service wired
- No password reset workflow
- No audit logging
- No rate limiting on login

## Production Upgrade Recommendations

- Add password hashing (BCrypt / ASP.NET Identity hasher)
- Introduce Roles (Admin/User)
- Add email verification tokens
- Add password reset tokens
- Add lockout policy
- Add logging + telemetry
- Move status to enum instead of string
- Add DTO mapping layer
- Add repository/service abstraction
## Project Structure

```text
AuthManager Enterprise/
├── Controllers/
│   ├── AccountController.cs         # Authentication logic (Login/Register)
│   ├── AdminController.cs           # Administrative & User management logic
│   └── HomeController.cs            # Default landing page logic
├── Data/
│   └── ApplicationDbContext.cs      # Entity Framework database context
├── Migrations/                      # Database schema migration files
├── Models/
│   ├── DomainModels/
│   │   └── User.cs                  # Core User data entity
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs        # Data for Login UI
│   │   ├── RegisterViewModel.cs     # Data for Registration UI
│   │   └── UserManagementViewModel.cs # Data for Admin dashboard
│   └── ErrorViewModel.cs            # Error handling model
├── Views/
│   ├── Account/                     # Auth views (Login, Register)
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Admin/                       # Admin dashboard views
│   │   └── Index.cshtml
│   ├── Home/                        # General site views
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Shared/                      # Reusable components & Layout
│   │   ├── _Layout.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── _ViewImports.cshtml          # Global namespaces & Tag Helpers
│   └── _ViewStart.cshtml            # Default layout settings
├── wwwroot/                         # Static files (CSS, JS, Libs)
├── appsettings.json                 # Main configuration (Conn Strings)
├── appsettings.Development.json     # Environment-specific settings
├── Dockerfile                       # Container deployment instructions
├── LICENSE                          # Project licensing terms
├── Program.cs                       # App entry point & service registration
└── README.md                        # Documentation