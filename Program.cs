using AuthManagerEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// =======================
// 1. DATABASE
// =======================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =======================
// 2. MVC
// =======================
builder.Services.AddControllersWithViews();

// =======================
// 3. COOKIE AUTH
// =======================
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";

        // IMPORTANT: Needed for Remember Me
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// =======================
// 4. PIPELINE
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// =======================
// 5. AUTHENTICATION
// =======================
app.UseAuthentication();

// =======================
// 6. GLOBAL BLOCK CHECK
// REQUIREMENT #5
// =======================
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    // NOTE: Skip login & register to avoid redirect loop
    if (path!.Contains("/account/login") || path.Contains("/account/register"))
    {
        await next();
        return;
    }

    if (context.User.Identity?.IsAuthenticated == true)
    {
        var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != null)
        {
            var user = await db.Users.FindAsync(int.Parse(userId));

            // IMPORTANT: Blocked or deleted users are logged out
            if (user == null || user.Status == "blocked")
            {
                await context.SignOutAsync("Cookies");
                context.Response.Redirect("/Account/Login");
                return;
            }
        }
    }

    await next();
});

// =======================
// 7. AUTHORIZATION
// =======================
app.UseAuthorization();

// =======================
// 8. ROUTING
// =======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
