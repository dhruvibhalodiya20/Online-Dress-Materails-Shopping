using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Mypro.Models;
using Mypro.Services;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<OrderCalculationService>();


// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();


////admin 
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//    if (!db.Customers.Any(c => c.Role == "Admin"))
//    {
//        var admin = new Customer
//        {
//            CustomerName = "Dhruvi",
//            Email = "dhruvibhalodiya20@gmail.com",
//            Password = HashPassword("Dhruvi@20"), // hash password
//            Role = "Admin",
//            CityId = 1 
//        };

//        db.Customers.Add(admin);
//        db.SaveChanges();
//    }

//    // Local function to hash password
//    static string HashPassword(string password)
//    {
//        using var sha = SHA256.Create();
//        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
//        return Convert.ToBase64String(bytes);
//    }
//}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session before Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();
