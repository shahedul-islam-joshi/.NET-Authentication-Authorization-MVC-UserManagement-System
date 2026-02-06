using AuthManagerEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. DYNAMIC CONFIGURATION (Local vs Render)
// ==========================================

// Handle Port: Render uses a $PORT variable. Local uses launchSettings.json.
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Handle Connection String: 
// 1. Checks appsettings.Development.json (Local)
// 2. Checks Environment Variables (Render)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =======================
// 2. MVC & AUTH SERVICES
// =======================
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// ==========================================
// 3. AUTO-MIGRATE: Run on Startup
// ==========================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Only migrate if we have a connection string to avoid startup crashes
        if (!string.IsNullOrEmpty(connectionString))
        {
            db.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        // This will show in your VS Debug Console or Render Logs
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

// =======================
// 4. MIDDLEWARE PIPELINE
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Keep Redirection off locally if you have SSL cert issues, 
// but it's generally safe for Render.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// =======================
// 5. GLOBAL BLOCK CHECK
// =======================
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";

    // Allow login/register without being blocked
    if (path.Contains("/account/login") || path.Contains("/account/register") || path.StartsWith("/lib") || path.StartsWith("/css"))
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();