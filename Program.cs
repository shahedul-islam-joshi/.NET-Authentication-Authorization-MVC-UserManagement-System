using AuthManagerEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. RENDER CONFIG: Dynamic Port & Connection
// ==========================================
// Tells the app to listen on the port Render provides
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Reads from Environment Variables (Render) OR appsettings (Local)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

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
        db.Database.Migrate(); // This creates tables on Somee automatically
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

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
app.UseAuthentication();

// =======================
// 5. GLOBAL BLOCK CHECK
// =======================
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";

    if (path.Contains("/account/login") || path.Contains("/account/register"))
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();