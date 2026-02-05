using Microsoft.AspNetCore.Mvc;
using AuthManagerEnterprise.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // REQUIREMENT #3: SORT BY LAST LOGIN
        var users = await _db.Users
            .OrderByDescending(u => u.LastLoginTime)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> BulkAction(int[] userIds, string action)
    {
        if (action == "DeleteUnverified")
        {
            // IMPORTANT: DELETE UNVERIFIED USERS
            var unverifiedUsers = await _db.Users
                .Where(u => u.Status == "unverified")
                .ToListAsync();

            _db.Users.RemoveRange(unverifiedUsers);
        }
        else
        {
            if (userIds == null || userIds.Length == 0)
                return RedirectToAction("Index");

            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            // NOTE: APPLY TOOLBAR ACTION
            foreach (var user in users)
            {
                if (action == "Block")
                    user.Status = "blocked";

                if (action == "Unblock")
                    user.Status = "active";
            }

            if (action == "Delete")
                _db.Users.RemoveRange(users);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
