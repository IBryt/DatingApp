using API.Data;
using API.Entities;
using API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests.TestHelpers;

public static class DataHelpers
{
    private static List<AppUser> users = (Seed.GetUsers()).GetAwaiter().GetResult().ToList();

    public static List<AppUser> GetUsers()
    {
      return DeepClone(users);
    }

    public static  DataContext GetDatabase()
    {
        var context = new DataContext(GetUnitTestDbContextOptions());

        foreach (var user in GetUsers())
        {
            context.Users.Add(user);
        }
        
        context.SaveChanges();

        return context;
    }

    public static IMapper GetMapperProfile()
    {
        var myProfile = new AutoMapperProfiles();
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));

        return new Mapper(configuration);
    }

    private static DbContextOptions GetUnitTestDbContextOptions()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return options;
    }

    public static T DeepClone<T>(T obj)
    {
        string jsonString = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}
