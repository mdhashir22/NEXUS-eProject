using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Services;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// MVC SERVICES
// =====================================================

builder.Services.AddControllersWithViews();


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


// =====================================================
// ASP.NET CORE IDENTITY
// =====================================================

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// =====================================================
// AUTHENTICATION COOKIE
// =====================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


// =====================================================
// NEXUS AI ASSISTANT
// =====================================================

builder.Services.AddHttpClient<INexusAiService, NexusAiService>();

builder.Services.AddScoped<IEmailService, EmailService>();


// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();


// =====================================================
// IDENTITY SEEDER
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await IdentitySeeder.SeedAsync(services);
}


// =====================================================
// HTTP REQUEST PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// GLOBAL MAINTENANCE MODE
// =====================================================

app.Use(async (context, next) =>
{
    var path = context.Request.Path;


    // -------------------------------------------------
    // Always allow Maintenance Page
    // -------------------------------------------------

    if (path.StartsWithSegments("/Home/Maintenance"))
    {
        await next();
        return;
    }


    // -------------------------------------------------
    // Always allow Login / Logout / Access Denied
    // so employees can still enter the system.
    // -------------------------------------------------

    if (path.StartsWithSegments("/Account/Login") ||
        path.StartsWithSegments("/Account/Logout") ||
        path.StartsWithSegments("/Account/AccessDenied"))
    {
        await next();
        return;
    }


    // -------------------------------------------------
    // Read Maintenance Mode directly from database
    // -------------------------------------------------

    var db =
        context.RequestServices
            .GetRequiredService<ApplicationDbContext>();


    var maintenanceMode =
        await db.SystemSettings
            .AsNoTracking()
            .Select(s => s.MaintenanceMode)
            .FirstOrDefaultAsync();


    // -------------------------------------------------
    // Website operational
    // -------------------------------------------------

    if (!maintenanceMode)
    {
        await next();
        return;
    }


    // -------------------------------------------------
    // Maintenance ON
    //
    // Admin and employees are allowed.
    // Public visitors and customers are blocked.
    // -------------------------------------------------

    var user = context.User;


    var isStaff =
        user.Identity?.IsAuthenticated == true &&
        (
            user.IsInRole("Admin") ||
            user.IsInRole("Retail Employee") ||
            user.IsInRole("Technical Employee") ||
            user.IsInRole("Accounts Employee")
        );


    if (isStaff)
    {
        await next();
        return;
    }


    // -------------------------------------------------
    // Public / Customer → Maintenance Page
    // -------------------------------------------------

    context.Response.Redirect("/Home/Maintenance");
});


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// DEFAULT MVC ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// =====================================================
// RUN APPLICATION
// =====================================================

app.Run();