using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;
using WebApplication2.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContextPool<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDBConnection"
)));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Service Email pour les notifications
builder.Services.AddScoped<IEmailService, EmailService>();
// Service de factures PDF
builder.Services.AddScoped<IInvoiceService, WebApplication2.Services.InvoiceService>();
// Service d'analytique
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
// Stripe service (server-side). Configure keys in appsettings or user secrets under Stripe:SecretKey and Stripe:WebhookSecret
builder.Services.AddScoped<IStripeService, StripeService>();
// PayPal service
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPayPalService, PayPalService>();
// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var stripeSecret = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrEmpty(stripeSecret))
{
    StripeConfiguration.ApiKey = stripeSecret;
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
