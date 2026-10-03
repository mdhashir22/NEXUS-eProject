using Microsoft.AspNetCore.Identity;

namespace NEXUS_eProject.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // =====================================================
            // NEXUS ROLES
            // =====================================================

            string[] roles =
            {
                "Admin",
                "Retail Employee",
                "Technical Employee",
                "Accounts Employee",
                "Customer"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            roleResult.Errors.Select(e => e.Description));

                        throw new Exception(
                            $"Could not create role '{role}': {errors}");
                    }
                }
            }


            // =====================================================
            // DEFAULT ADMIN ACCOUNT
            // =====================================================

            const string adminEmail = "admin@nexus.com";
            const string adminPassword = "Admin@123";

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);


            // =====================================================
            // CREATE ADMIN IF NOT EXISTS
            // =====================================================

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var userResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        userResult.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Could not create Admin account: {errors}");
                }
            }


            // =====================================================
            // ASSIGN ADMIN ROLE
            // =====================================================

            if (!await userManager.IsInRoleAsync(
                adminUser,
                "Admin"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "Admin");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Could not assign Admin role: {errors}");
                }
            }
        }
    }
}