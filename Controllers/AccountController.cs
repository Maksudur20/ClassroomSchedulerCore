using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ClassroomSchedulerCore.Models;
using System.Threading.Tasks;
using ClassroomSchedulerCore.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ClassroomSchedulerCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Role = model.Role,
                        EmailConfirmed = true // Auto-confirm for simplicity
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        // Ensure the role exists
                        string roleName = user.Role.ToString();
                        if (!await _roleManager.RoleExistsAsync(roleName))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(roleName));
                            _logger.LogInformation($"Created role {roleName} as it did not exist.");
                        }

                        // Add the user to the role
                        await _userManager.AddToRoleAsync(user, roleName);
                        _logger.LogInformation($"User added to role {roleName}");

                        // Sign in the user right away
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error during registration");
                    ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
