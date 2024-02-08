using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Tests.TestHelpers;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Staging);

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DataContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDatabase");
            });

            InitData(services).GetAwaiter().GetResult();
        });
    }


    private async Task InitData(IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var scopedServices = scope.ServiceProvider;
        var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<AppRole>>();
        await Seed.SeedUsers(userManager, roleManager);
        await DataHelpers.AddModerator(userManager, roleManager);
    }
}