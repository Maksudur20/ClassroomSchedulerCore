using ClassroomSchedulerCore.Data;
using ClassroomSchedulerCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ClassroomSchedulerCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Display summary statistics on the dashboard
            ViewBag.TotalRooms = await _context.Rooms.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.EmergencyBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Emergency);
            
            // Get today's bookings for quick reference
            var today = DateTime.Today;
            var tomorrowDate = today.AddDays(1);
            var todaysBookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.StartTime >= today && b.StartTime < tomorrowDate)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
            
            return View(todaysBookings);
        }
        
        public async Task<IActionResult> Schedule()
        {
            // Get all rooms and bookings for the schedule view
            var rooms = await _context.Rooms.ToListAsync();
            var today = DateTime.Today;
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.StartTime.Date >= today)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
                
            ViewBag.Rooms = rooms;
            return View(bookings);
        }
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _context.AuditLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(100) // Limit to prevent performance issues
                .ToListAsync();
                
            return View(logs);
        }
        
        public IActionResult About()
        {
            ViewBag.Message = "Classroom Resource Scheduler";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        // GET: /Home/FixPermissions - Diagnostic page to show and fix student permissions
        public async Task<IActionResult> FixPermissions(bool isFixed = false)
        {
            // Get current user information
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Redirect to login page if not logged in
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Pass information to the view
            ViewBag.UserName = user.UserName;
            ViewBag.UserId = userId;
            ViewBag.UserRoles = userRoles.ToList();
            ViewBag.IsInStudentRole = userRoles.Contains("Student");
            ViewBag.IsFixed = isFixed;
            
            return View();
        }
        
        // POST: /Home/ApplyFix - Direct action to apply permissions fix
        [HttpPost]
        public async Task<IActionResult> ApplyFix()
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge(); // Redirect to login if not logged in
                }
                
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                
                // Ensure the Student role exists
                if (!(await _roleManager.RoleExistsAsync("Student")))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }
                
                // Add user to Student role directly
                if (!(await _userManager.IsInRoleAsync(user, "Student")))
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }
                
                // Mark the user as Student role in the application user
                if (user.Role != UserRole.Student)
                {
                    user.Role = UserRole.Student;
                    await _userManager.UpdateAsync(user);
                }
                
                return RedirectToAction(nameof(FixPermissions), new { isFixed = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying permission fix");
                TempData["ErrorMessage"] = $"Error fixing permissions: {ex.Message}";
                return RedirectToAction(nameof(FixPermissions));
            }
        }
        
        // This action will fix the student booking permissions issue
        public async Task<IActionResult> FixStudentPermissions()
        {
            try
            {
                // Find the student user
                var studentUser = await _userManager.FindByEmailAsync("student@example.com");
                if (studentUser == null)
                {   
                    // Create the student user if it doesn't exist
                    studentUser = new ApplicationUser
                    {
                        UserName = "student@example.com",
                        Email = "student@example.com",
                        FirstName = "Student",
                        LastName = "User",
                        Role = UserRole.Student,
                        EmailConfirmed = true
                    };
                    
                    await _userManager.CreateAsync(studentUser, "Student123!");
                }
                
                // Ensure the Student role exists
                if (!(await _roleManager.RoleExistsAsync("Student")))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }
                
                // Add the student user to the Student role if needed
                if (!(await _userManager.IsInRoleAsync(studentUser, "Student")))
                {
                    await _userManager.AddToRoleAsync(studentUser, "Student");
                }
                
                TempData["SuccessMessage"] = "Student permissions have been fixed successfully. You should now be able to book classrooms as a student."; 
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing student permissions");
                TempData["ErrorMessage"] = $"Error fixing permissions: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
