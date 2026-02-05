using Microsoft.AspNetCore.Mvc;
using AuthManagerEnterprise.Data;
using AuthManagerEnterprise.Models.DomainModels;
using AuthManagerEnterprise.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    // =========================
    // REGISTER
    // =========================
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Password = model.Password, // IMPORTANT: Plain password (allowed for task)
            Status = "unverified",
            RegistrationTime = DateTime.Now
        };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // IMPORTANT: UNIQUE INDEX CHECKS HERE

            TempData["Success"] = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }
        catch (DbUpdateException)
        {
            // NOTE: Uniqueness is enforced by the database
            ModelState.AddModelError("", "Email already exists.");
            return View(model);
        }
    }

    // =========================
    // LOGIN
    // =========================
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == model.Email && u.Password == model.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        if (user.Status == "blocked")
        {
            ModelState.AddModelError("", "Your account is blocked.");
            return View(model);
        }

        // IMPORTANT: Update last login time
        user.LastLoginTime = DateTime.Now;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };

        var identity = new ClaimsIdentity(claims, "Cookies");

        // =========================
        // REMEMBER ME IMPLEMENTATION
        // =========================
        await HttpContext.SignInAsync(
            "Cookies",
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                // IMPORTANT: If RememberMe is checked,
                // the cookie survives browser restart
                IsPersistent = model.RememberMe,

                // NOTE: Cookie expiration when Remember Me is enabled
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : null
            });

        return RedirectToAction("Index", "Admin");
    }

    // =========================
    // LOGOUT
    // =========================
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }
}
