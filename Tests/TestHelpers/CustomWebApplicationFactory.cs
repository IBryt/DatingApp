using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using API.Interfaces;

namespace Tests.TestHelpers;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    public IPhotoService PhotoService { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Staging);

        builder.ConfigureServices(services =>
        {
            RemoveDescriptor(services, typeof(DbContextOptions<DataContext>));
            RemoveDescriptor(services, typeof(DbConnection));

            if (PhotoService != null)
            {
                RemoveDescriptor(services, typeof(IPhotoService));
                services.AddSingleton<IPhotoService>(PhotoService);
            }

            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("InMemoryDatabase"));

            InitData(services).GetAwaiter().GetResult();
        });
    }

    private void RemoveDescriptor(IServiceCollection services, Type type)
    {
        var descriptor = services.SingleOrDefault(
           d => d.ServiceType == type);

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
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