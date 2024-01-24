using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync())
        {
            return;
        }

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var users = JsonSerializer.Deserialize<IEnumerable<AppUser>>(userData);
        foreach (var user in users)
        {
            user.UserName = user.UserName.ToLower();

            await userManager.CreateAsync(user, "Pa$$w0rd");
        }
    }
}
