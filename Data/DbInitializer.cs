using ClassroomSchedulerCore.Models;
using Microsoft.AspNetCore.Identity;

namespace ClassroomSchedulerCore.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Initialize database if needed
            context.Database.EnsureCreated();

            // Check if roles exist, create them if they don't
            string[] roleNames = { "Admin", "Faculty", "Student", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Check if admin user exists, create if it doesn't
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Role = UserRole.Admin,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Check if faculty user exists, create if it doesn't
            if (await userManager.FindByEmailAsync("faculty@example.com") == null)
            {
                var facultyUser = new ApplicationUser
                {
                    UserName = "faculty@example.com",
                    Email = "faculty@example.com",
                    FirstName = "Faculty",
                    LastName = "Member",
                    Role = UserRole.Faculty,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(facultyUser, "Faculty123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(facultyUser, "Faculty");
                }
            }

            // Check if student user exists, create if it doesn't
            if (await userManager.FindByEmailAsync("student@example.com") == null)
            {
                var studentUser = new ApplicationUser
                {
                    UserName = "student@example.com",
                    Email = "student@example.com",
                    FirstName = "Student",
                    LastName = "User",
                    Role = UserRole.Student,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, "Student123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }

            // Check if regular user exists, create if it doesn't
            if (await userManager.FindByEmailAsync("user@example.com") == null)
            {
                var regularUser = new ApplicationUser
                {
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    FirstName = "Regular",
                    LastName = "User",
                    Role = UserRole.User,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }

            // Check if rooms exist, create sample rooms if they don't
            if (!context.Rooms.Any())
            {
                var rooms = new Room[]
                {
                    new Room
                    {
                        Name = "Lecture Hall A",
                        Location = "Main Building, Floor 1",
                        Capacity = 100,
                        HasProjector = true,
                        HasComputers = false,
                        Description = "Large lecture hall with projector and sound system"
                    },
                    new Room
                    {
                        Name = "Seminar Room B",
                        Location = "Science Building, Floor 2",
                        Capacity = 30,
                        HasProjector = true,
                        HasComputers = false,
                        Description = "Medium-sized seminar room with whiteboard and projector"
                    },
                    new Room
                    {
                        Name = "Computer Lab C",
                        Location = "Technology Building, Floor 3",
                        Capacity = 25,
                        HasProjector = true,
                        HasComputers = true,
                        Description = "Computer lab with 25 workstations and instructor podium"
                    }
                };

                context.Rooms.AddRange(rooms);
                await context.SaveChangesAsync();
            }

            // Add sample bookings if they don't exist
            if (!context.Bookings.Any())
            {
                var adminUser = await userManager.FindByEmailAsync("admin@example.com");
                var facultyUser = await userManager.FindByEmailAsync("faculty@example.com");
                var rooms = context.Rooms.ToList();

                if (adminUser != null && facultyUser != null && rooms.Count >= 3)
                {
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);

                    var bookings = new Booking[]
                    {
                        new Booking
                        {
                            RoomId = rooms[0].Id,
                            UserId = facultyUser.Id,
                            Title = "Introduction to Computer Science",
                            Description = "First lecture of the semester",
                            StartTime = today.AddHours(9),
                            EndTime = today.AddHours(11),
                            Status = BookingStatus.Reserved,
                            IsEmergency = false,
                            CreatedAt = DateTime.Now.AddDays(-7)
                        },
                        new Booking
                        {
                            RoomId = rooms[1].Id,
                            UserId = facultyUser.Id,
                            Title = "Physics Seminar",
                            Description = "Weekly seminar for physics majors",
                            StartTime = today.AddHours(13),
                            EndTime = today.AddHours(14.5),
                            Status = BookingStatus.Available,
                            IsEmergency = false,
                            CreatedAt = DateTime.Now.AddDays(-3)
                        },
                        new Booking
                        {
                            RoomId = rooms[2].Id,
                            UserId = adminUser.Id,
                            Title = "Emergency IT Maintenance",
                            Description = "Critical software update",
                            StartTime = today.AddHours(15),
                            EndTime = today.AddHours(17),
                            Status = BookingStatus.Emergency,
                            IsEmergency = true,
                            CreatedAt = DateTime.Now.AddDays(-1)
                        },
                        new Booking
                        {
                            RoomId = rooms[0].Id,
                            UserId = facultyUser.Id,
                            Title = "Software Engineering Lecture",
                            Description = "Agile methodologies overview",
                            StartTime = tomorrow.AddHours(10),
                            EndTime = tomorrow.AddHours(12),
                            Status = BookingStatus.Reserved,
                            IsEmergency = false,
                            CreatedAt = DateTime.Now.AddDays(-2)
                        },
                        new Booking
                        {
                            RoomId = rooms[1].Id,
                            UserId = facultyUser.Id,
                            Title = "Mathematics Study Group",
                            Description = "Weekly study session",
                            StartTime = tomorrow.AddHours(14),
                            EndTime = tomorrow.AddHours(16),
                            Status = BookingStatus.Available,
                            IsEmergency = false,
                            CreatedAt = DateTime.Now.AddDays(-4)
                        }
                    };

                    context.Bookings.AddRange(bookings);
                    await context.SaveChangesAsync();

                    // Add sample audit logs
                    var auditLogs = new AuditLog[]
                    {
                        new AuditLog
                        {
                            UserId = adminUser.Id,
                            UserName = adminUser.UserName ?? "admin@example.com",
                            Action = "Create",
                            EntityName = "Room",
                            EntityId = rooms[0].Id,
                            Details = "Created room Lecture Hall A",
                            Timestamp = DateTime.Now.AddDays(-10)
                        },
                        new AuditLog
                        {
                            UserId = facultyUser.Id,
                            UserName = facultyUser.UserName ?? "faculty@example.com",
                            Action = "Create",
                            EntityName = "Booking",
                            EntityId = 1,
                            Details = "Created booking Introduction to Computer Science",
                            Timestamp = DateTime.Now.AddDays(-7)
                        }
                    };

                    context.AuditLogs.AddRange(auditLogs);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
