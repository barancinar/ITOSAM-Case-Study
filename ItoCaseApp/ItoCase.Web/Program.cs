using ItoCase.Core.Entities;
using ItoCase.Core.Interfaces;
using ItoCase.Infrastructure.Persistence.Context;
using ItoCase.Infrastructure.Repositories;
using ItoCase.Service.Services;
using ItoCase.Service.Strategies;
using ItoCase.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ItoCaseDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ItoCaseDbContext>()
.AddDefaultTokenProviders();

// Repository Pattern
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// Unit of Work Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Strategy Pattern for Charts
builder.Services.AddScoped<IChartStrategy, BooksByCategoryStrategy>();
builder.Services.AddScoped<IChartStrategy, BestSellersStrategy>();
builder.Services.AddScoped<ChartService>();


// Application Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddAutoMapper(typeof(ItoCase.Service.Mappings.MapProfile));




// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        // Hata olursa log yazdır
        Console.WriteLine("Seed hatası: " + ex.Message);
    }
}

app.Run();
