using API.Data;
using API.Entities;
using API.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests.TestHelpers;

public static class DataHelpers
{
    public static  DataContext GetDatabase()
    {
        return new DataContext(GetUnitTestDbContextOptions());
    }

    public static void AddUsers(DataContext context)
    {
        foreach (var user in Seed.GetUsers())
        {
            context.Users.Add(user);
        }

        context.SaveChanges();
    }

    public static IMapper GetMapperProfile()
    {
        var myProfile = new AutoMapperProfiles();
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));

        return new Mapper(configuration);
    }

    public static async Task AddModerator (UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {

        var admin = new AppUser
        {
            UserName = "moderator",
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, new[] { "Moderator" });
    }

    private static DbContextOptions GetUnitTestDbContextOptions()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return options;
    }
}
