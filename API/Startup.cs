using API.Extensions;
using API.Middleware;

namespace API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices(Configuration);
        services.AddControllers();
        services.AddCors();
        services.AddIdentityService(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors(policy => policy
            .WithOrigins("https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
