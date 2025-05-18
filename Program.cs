using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClassroomSchedulerCore.Data;
using ClassroomSchedulerCore.Models;
using ClassroomSchedulerCore.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using ClassroomSchedulerCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Add email sender service (using a dummy implementation for now)
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

// Configure authorization with simplified policies
builder.Services.AddAuthorization(options =>
{
    // Student permission policy - used for booking classrooms
    options.AddPolicy("StudentBookingPolicy", policy =>
    {
        // Allow any authenticated user to create bookings (we'll handle specific restrictions in the controller)
        policy.RequireAuthenticatedUser();
    });
    
    // Admin only policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
        
    // Do not set a fallback policy so anonymous access is allowed by default
    // This allows users to access registration pages without authentication
    options.FallbackPolicy = null;
    
    // Allow anonymous access to account-related actions
    options.AddPolicy("AllowAnonymous", policy => 
        policy.RequireAssertion(_ => true));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Enable Identity UI Pages
app.MapControllers();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
