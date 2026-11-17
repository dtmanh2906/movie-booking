using CinemaBooking.Domain.Constants;
using CinemaBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Infrastructure.Persistence;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<User> userManager,
        string adminEmail,
        string? adminPassword)
    {
        // Tạo hai vai trò nếu chưa tồn tại
        foreach (var role in new[] { Roles.Admin, Roles.Customer })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole<int>(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Không tạo được vai trò: " +
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Chưa có mật khẩu cấu hình thì không tạo Admin
        if (string.IsNullOrWhiteSpace(adminPassword))
            return;

        // Không tạo tài khoản Admin trùng email
        if (await userManager.FindByEmailAsync(adminEmail)
            is not null)
            return;

        var admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản trị viên",
            EmailConfirmed = true
        };

        // Identity tự băm mật khẩu
        var resultCreate = await userManager.CreateAsync(
            admin, adminPassword);

        if (!resultCreate.Succeeded)
        {
            throw new InvalidOperationException(
                "Không tạo được tài khoản Admin: " +
                string.Join("; ",
                    resultCreate.Errors.Select(e => e.Description)));
        }

        // Gán vai trò Admin
        var resultRole = await userManager.AddToRoleAsync(
            admin, Roles.Admin);

        if (!resultRole.Succeeded)
        {
            throw new InvalidOperationException(
                "Không gán được vai trò Admin: " +
                string.Join("; ",
                    resultRole.Errors.Select(e => e.Description)));
        }
    }
}