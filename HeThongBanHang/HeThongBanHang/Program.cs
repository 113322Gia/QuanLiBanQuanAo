using HeThongBanHang.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<QuanLiBanQuanAoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session cần được đặt trước Authorization
app.UseSession();

app.UseAuthorization();

// Routing cho Area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=HomeAdmin}/{action=Index}/{id?}");

// Routing mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
