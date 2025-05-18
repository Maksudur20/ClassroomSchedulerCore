using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClassroomSchedulerCore.Models;
using System.Threading.Tasks;

namespace ClassroomSchedulerCore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: /Roles/SyncRoles
        public async Task<IActionResult> SyncRoles()
        {
            // This action explicitly syncs the Student role to have booking permissions
            var studentRole = await _roleManager.FindByNameAsync("Student");
            if (studentRole == null)
            {
                // Create the role if it doesn't exist
                await _roleManager.CreateAsync(new IdentityRole("Student"));
            }

            // Find the student user
            var studentUser = await _userManager.FindByEmailAsync("student@example.com");
            if (studentUser != null)
            {
                // Make sure student is in the Student role
                if (!await _userManager.IsInRoleAsync(studentUser, "Student"))
                {
                    await _userManager.AddToRoleAsync(studentUser, "Student");
                }
            }

            TempData["Message"] = "Student role permissions have been synchronized successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}
