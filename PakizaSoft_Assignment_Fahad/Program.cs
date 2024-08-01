using Microsoft.EntityFrameworkCore;
using PakizaSoft_Assignment_Fahad.HostedServices;
using PakizaSoft_Assignment_Fahad.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ProductDbContext>(op => op.UseSqlServer(builder.Configuration.GetConnectionString("db")));

builder.Services.AddScoped<DbSeeder>();

builder.Services.AddHostedService<ProductDbSeederHostedService>();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
