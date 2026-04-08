using SV22T1020674.DataLayers.SQLServer;
using SV22T1020674.DataLayers.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ===== Cấu hình Session =====
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== Thêm services MVC =====
builder.Services.AddControllersWithViews();

// ===== Đăng ký các Repository với DI =====
string defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Server=ADMIN-PC;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;";

// Customer
builder.Services.AddScoped<ICustomerRepository>(sp =>
    new CustomerRepository(defaultConnection));

// Product
builder.Services.AddScoped<IProductRepository>(sp =>
    new ProductRepository(defaultConnection));

// Order
builder.Services.AddScoped<IOrderRepository>(sp =>
    new OrderRepository(defaultConnection));



// ===== Khởi tạo cấu hình Business Layer =====
SV22T1020674.BusinessLayers.Configuration.Initialize(defaultConnection);

// ===== Build ứng dụng =====
var app = builder.Build();

// ===== Cấu hình pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

// ===== Routing =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
