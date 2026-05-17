using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// Register EF Core with SQL Server
builder.Services.AddDbContext<GLMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register our services (Strategy pattern from Part 1)
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
