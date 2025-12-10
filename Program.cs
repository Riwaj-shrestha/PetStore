using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PetStore.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// Register services
// -----------------------------------------

// MVC with views
builder.Services.AddControllersWithViews();

// Register AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Session (used for Login state and CartID)
builder.Services.AddSession(options =>
{
    // Session timeout (here: 30 minutes of inactivity)
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register HttpContextAccessor so we can inject it into views (_Layout.cshtml)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// -----------------------------------------
// Configure HTTP request pipeline
// -----------------------------------------

if (!app.Environment.IsDevelopment())
{
    // Use the default error handler and HSTS in production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session BEFORE Authorization
app.UseSession();

app.UseAuthorization();

// Default route configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed initial data (if you have DbSeeder)
try
{
    DbSeeder.SeedDatabase(app);
    System.Diagnostics.Debug.WriteLine("Database seeding completed");
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error during database seeding: {ex.Message}");
}

app.Run();
