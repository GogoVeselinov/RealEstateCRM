using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Data;
using RealEstateCRM.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IDocumentGeneratorService, DocumentGeneratorService>();

// Add Razor runtime compilation for hot reload in development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

var app = builder.Build();

// Еднократно: миграции + сийд
using (var scope = app.Services.CreateScope())
{
    var init = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await init.InitializeAsync();
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Enable detailed errors and hot reload in development
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
