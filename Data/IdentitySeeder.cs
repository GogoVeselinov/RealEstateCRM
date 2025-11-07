using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RealEstateCRM.Models.Identity;

namespace RealEstateCRM.Data;

public static class IdentitySeeder
{
    public static async Task<(ApplicationUser admin, ApplicationUser manager, ApplicationUser guest)> SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create roles if they don't exist
        await EnsureRoleAsync(roleManager, AppRoles.Admin);
        await EnsureRoleAsync(roleManager, AppRoles.Manager);
        await EnsureRoleAsync(roleManager, AppRoles.Guest);

        // Create users if they don't exist
        var admin = await EnsureUserAsync(userManager, "admin@company.com", "Admin123!", AppRoles.Admin, "Administrator");
        var manager = await EnsureUserAsync(userManager, "agent@company.com", "Agent123!", AppRoles.Manager, "Real Estate Agent");
        var guest = await EnsureUserAsync(userManager, "guest@company.com", "Guest123!", AppRoles.Guest, "Guest User");

        return (admin, manager, guest);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string fullName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        return user;
    }
}