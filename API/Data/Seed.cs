using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Data;

public static class Seed
{
    private static List<AppUser> usersFromFile = (GetUsersFromFile()).GetAwaiter().GetResult().ToList();

    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync())
        {
            return;
        }

        var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        var users = DeepClone(usersFromFile);

        foreach (var user in users)
        {
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser
        {
            UserName = "admin",
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }

    private static async Task<IEnumerable<AppUser>> GetUsersFromFile()
    {
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var users = JsonSerializer.Deserialize<IEnumerable<AppUser>>(userData);

        foreach (var user in users)
        {
            user.UserName = user.UserName.ToLower();
            user.DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc);
            user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
            user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);
        }

        return users;
    }

    public static List<AppUser> GetUsers()
    {
        return DeepClone(usersFromFile);
    }

    private static T DeepClone<T>(T obj)
    {
        string jsonString = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}
