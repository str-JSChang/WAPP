using Microsoft.EntityFrameworkCore;
using Muzik67;
using Muzik67.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddScoped<ModuleDataService>();

// Configure SQLite database context
builder.Services.AddDbContext<MuzikDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=Muzik67.db"));

// Configure session state for login authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Auto-seed the SQLite database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MuzikDbContext>();
    DbInitializer.Initialize(context);
}

// Simple HTTP request pipeline (assignment style)
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
