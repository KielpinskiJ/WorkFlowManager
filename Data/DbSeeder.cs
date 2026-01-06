using Microsoft.AspNetCore.Identity;
using WorkFlowManager.Models;
using System;

namespace WorkFlowManager.Data;

public static class DbSeeder
{
    public const string AdminEmail = "admin@wsb.pl";
    private static readonly string AdminPassword =
        Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
        ?? throw new InvalidOperationException("Admin password is not configured. Set the ADMIN_PASSWORD environment variable.");
    
    public const string RoleAdmin = "Admin";
    public const string RoleEmployee = "Employee";

    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create roles
        await CreateRoleIfNotExists(roleManager, RoleAdmin);
        await CreateRoleIfNotExists(roleManager, RoleEmployee);

        // Create admin user
        await CreateAdminUserIfNotExists(userManager);
    }

    private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task CreateAdminUserIfNotExists(UserManager<ApplicationUser> userManager)
    {
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FirstName = "Admin",
                LastName = "System",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, AdminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleAdmin);
            }
        }
    }
}
