using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.DataLayers.SQLServer;
using SV22T1020674.Models.Partner;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option =>
                {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1020674.Admin";
                    option.LoginPath = "/Account/Login";
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(7);
                    option.SlidingExpiration = true;
                    option.Cookie.HttpOnly = true;
                    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

// Configure Session
builder.Services.AddSession();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

// ===== ADD REPOSITORY (FIX LỖI CHÍNH) =====
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");

builder.Services.AddScoped<IUserAccountRepository>(sp => new UserAccountRepository(connectionString));
builder.Services.AddScoped<IProductRepository>(sp => new ProductRepository(connectionString));
builder.Services.AddScoped<IOrderRepository>(sp => new OrderRepository(connectionString));
builder.Services.AddScoped<IEmployeeRepository>(sp => new EmployeeRepository(connectionString));
builder.Services.AddScoped<ICustomerRepository>(sp =>
    new CustomerRepository(connectionString));
builder.Services.AddScoped<IGenericRepository<Shipper>>(sp =>
    new ShipperRepository(connectionString));




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

// Load static file từ Shop
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "..", "SV22T1020674.Shop", "wwwroot")
    ),
    RequestPath = "/shop"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Culture
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Application Context
SV22T1020674.Admin.ApplicationContext.Configure
(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

// PageSize
try
{
    SV22T1020674.Admin.ApplicationContext.PageSize = int.Parse(app.Configuration["PageSize"] ?? "10");
}
catch
{
    SV22T1020674.Admin.ApplicationContext.PageSize = 10;
}

// Initialize Business Layer
SV22T1020674.BusinessLayers.Configuration.Initialize(connectionString);

app.Run();
