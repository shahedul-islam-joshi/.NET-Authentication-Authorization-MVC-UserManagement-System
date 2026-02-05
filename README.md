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
```
AuthManagerEnterprise
│
├── Controllers
│   ├── AccountController.cs
│   └── AdminController.cs
│
├── Data
│   └── ApplicationDbContext.cs
│
├── Models
│   └── DomainModels
│       └── User.cs
│
├── ViewModels
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── UserManagementViewModel.cs
│
├── Views
│   ├── Account
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Admin
│   │   └── Index.cshtml
│   └── Shared
│       └── _Layout.cshtml
│
└── Program.cs
```